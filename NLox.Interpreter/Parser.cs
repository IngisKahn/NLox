namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;
using System;

public partial class Parser
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
            return await (this.Match(TokenType.Var) 
                              ? VariableDeclaration() 
                              : this.Match(TokenType.Fun) 
                                ? Function("function") 
                                : Statement());
        }
        catch (ParsingException)
        {
            this.Synchronize();
            return null;
        }
    }

    private Task<IStatement> Statement() =>
        this.Match(TokenType.Break, TokenType.Continue) ? this.ControlFlowStatement() :
        this.Match(TokenType.For) ? this.For() :
        this.Match(TokenType.If) ? this.If() :
        this.Match(TokenType.Print) ? this.PrintStatement() :
        this.Match(TokenType.Return) ? this.ReturnStatement() :
        this.Match(TokenType.While) ? this.WhileStatement() :
        this.Match(TokenType.LeftBrace) ? this.Block() : this.ExpressionStatement();

    public Task<IExpression> Expression() => this.Comma();

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
