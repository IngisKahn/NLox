﻿namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using System;

public class Parser
{
    private readonly IList<Token> tokens;
    private readonly Func<Token, string, Task>? error;
    private int current = 0;

    public Parser(IList<Token> tokens, Func<Token, string, Task>? error)
    {
        this.tokens = tokens;
        this.error = error;
    }
    public async Task<IExpression?> Parse()
    {
        try
        {
            return await this.Expression();
        }
        catch (ParsingException)
        {
            return null;
        }
    }

    private Task<IExpression> Expression() => this.Comma();
    private async Task<IExpression> Comma()
    {
        var expression = await this.Ternary();

        while (this.Match(TokenType.Comma))
        {
            var @operator = this.Previous;
            var right = await this.Ternary();
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
        if (!this.IsAtEnd) current++;
        return this.Previous;
    }

    private bool IsAtEnd => this.Peek.Type == TokenType.EoF;

    private Token Peek => this.tokens[current];

    private Token Previous => this.tokens[current - 1];

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
