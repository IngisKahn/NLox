namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;
using System;

public class Parser
{
    private readonly IList<Token> tokens;
    private readonly Func<Token, string, Task>? error;
    public int Current { get; set; }

    public Parser(IList<Token> tokens, Func<Token, string, Task>? error)
    {
        this.tokens = tokens;
        this.error = error;
    }

    public async IAsyncEnumerable<IStatement?> Parse()
    {
        while (!this.IsAtEnd)
            yield return await this.Declaration();
    }

    private async Task<IStatement?> Declaration()
    {
        try
        {
            return await (this.Match(TokenType.Var) ? VariableDeclaration() : Statement());
        }
        catch (ParsingException)
        {
            this.Synchronize();
            return null;
        }
    }

    private async Task<IStatement> If()
    {
        await this.Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = await this.Expression();
        await this.Consume(TokenType.RightParen, "Expect ')' after if condition.");

        var thenBranch = await this.Statement();
        return this.Match(TokenType.Else) ? new If(condition, thenBranch, await this.Statement()) :
            new If(condition, thenBranch);
    }

    private Task<IStatement> Statement() => 
        this.Match(TokenType.If) ? this.If() :
        this.Match(TokenType.Print) ? this.PrintStatement() :  
        this.Match(TokenType.LeftBrace) ? this.Block() : this.ExpressionStatement();

    private async Task<IStatement> Block()
    {
        List<IStatement> statements = new();

        while (!Check(TokenType.RightBrace) && !this.IsAtEnd)
        {
            var statement = await Declaration();
            if (statement != null) 
                statements.Add(statement);
        }

        await this.Consume(TokenType.RightBrace, "Expect '}' after block.");

        return new Block(statements);
    }

    private async Task<IStatement> PrintStatement()
    {
        var value = await this.Expression();
        if (value == null)
            throw new ParsingException("Expect expression after 'print'");
        await this.Consume(TokenType.Semicolon, "Expect ';' after expression");
        return new PrintStatement(value);
    }
    private async Task<IStatement> ExpressionStatement()
    {
        var value = await this.Expression();
        await this.Consume(TokenType.Semicolon, "Expect ';' after expression");
        return new ExpressionStatement(value);
    }

    public Task<IExpression> Expression() => this.Comma();
    private async Task<IExpression> Comma()
    {
        var expression = await this.Assignment();

        while (this.Match(TokenType.Comma))
        {
            var @operator = this.Previous;
            var right = await this.Assignment();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }
    private async Task<IExpression> Ternary()
    {
        Stack<IExpression> expressions = new();
        expressions.Push(await this.Equality());

        while (this.Match(TokenType.Question))
        {
            expressions.Push(await this.Expression());
            await this.Consume(TokenType.Colon, "Missing false condition of ternary expression");
            expressions.Push(await this.Equality());
        }

        while (expressions.Count > 1)
        {
            var right = expressions.Pop();
            var left = expressions.Pop();
            expressions.Push(new Ternary(expressions.Pop(), left, right));
        }

        return expressions.Pop();
    }

    private async Task<IExpression> Equality()
    {
        var expression = await this.Comparison();

        while (this.Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var @operator = this.Previous;
            var right = await this.Comparison();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
            if (this.Check(type))
            {
                this.Advance();
                return true;
            }

        return false;
    }
    private bool Check(TokenType type) => !this.IsAtEnd && this.Peek.Type == type;

    private Token Advance()
    {
        if (!this.IsAtEnd) 
            Current++;
        return this.Previous;
    }

    private bool IsAtEnd => this.Peek.Type == TokenType.EoF;

    private Token Peek => this.tokens[Current];

    private Token Previous => this.tokens[Current - 1];


    private async Task<IExpression> Comparison()
    {
        var expression = await this.Term();

        while (this.Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var @operator = this.Previous;
            var right = await this.Term();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private async Task<IExpression> Term()
    {
        var expression = await this.Factor();

        while (this.Match(TokenType.Minus, TokenType.Plus))
        {
            var @operator = this.Previous;
            var right = await this.Factor();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }
    private async Task<IExpression> Factor()
    {
        var expression = await this.Unary();

        while (this.Match(TokenType.Slash, TokenType.Star))
        {
            var @operator = this.Previous;
            var right = await this.Unary();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }
    private async Task<IExpression> Unary()
    {
        if (this.Match(TokenType.Bang, TokenType.Minus))
        {
            var @operator = this.Previous;
            var right = await this.Unary();
            return new Unary(@operator, right);
        }

        return await this.Primary();
    }

    private async Task<IStatement> VariableDeclaration()
    {
        var name = await this.Consume(TokenType.Identifier, "Expect variable name.");

        var initializer = this.Match(TokenType.Equal) ? await this.Expression() : null;

        await this.Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStatement(name, initializer);
    }

    private async Task<IExpression> Assignment()
    {
        var expression = await this.Ternary();

        if (!this.Match(TokenType.Equal))
            return expression;

        var equals = this.Previous;
        var value = await this.Assignment();

        if (expression is Variable v)
            return new Assign(v.Name, value);

        await Error(equals, "Invalid assignment target.");
        return expression;
    }


    private async Task<IExpression> Primary()
    {
        if (this.Match(TokenType.False))
            return new Literal(false);
        if (this.Match(TokenType.True))
            return new Literal(true);
        if (this.Match(TokenType.Nil))
            return new Literal(null);

        if (this.Match(TokenType.Number, TokenType.String))
            return new Literal(this.Previous.Literal);

        if (this.Match(TokenType.Identifier))
            return new Variable(this.Previous);

        if (this.Match(TokenType.LeftParen))
        {
            var expr = await this.Expression();
            await this.Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expr);
        }
        throw await this.Error(this.Peek, "Expect expression.");
    }
    private async Task<Token> Consume(TokenType type, string message) =>
        this.Check(type) ? this.Advance() : throw await this.Error(this.Peek, message);

    private async Task<ParsingException> Error(Token token, string message)
    {
        await (this.error?.Invoke(token, message) ?? Task.CompletedTask);
        return new ParsingException();
    }
    private void Synchronize()
    {
        this.Advance();

        while (!this.IsAtEnd)
        {
            if (this.Previous.Type == TokenType.Semicolon)
                return;

            switch (this.Peek.Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            this.Advance();
        }
    }
}
