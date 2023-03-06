namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;

public class Interpreter
{
    public void Interpret(Statement statement) => this.EvaluateStatement(statement);


    private void EvaluateStatement(Statement statement) => this.EvaluateStatement((dynamic)statement);
    private void EvaluateStatement(ExpressionStatement expressionStatement) => this.Evaluate(expressionStatement.Expression);

    private void EvaluateStatement(PrintStatement printStatement) =>
        Console.WriteLine(Stringify(this.Evaluate(printStatement.Expression)));

    public object? Evaluate(IExpression expression) => this.Evaluate((dynamic)expression);

    private object? Evaluate(Literal literal) => literal.Value;
    private object? Evaluate(Grouping grouping) => this.Evaluate(grouping.Expression);
    private object? Evaluate(Unary unary)
    {
        var right = this.Evaluate(unary.Right);

        return unary.Operator.Type == TokenType.Minus && right is not double
            ? throw new RuntimeException(unary.Operator, "Unary negation applied to non-number")
            : unary.Operator.Type switch
            {
                TokenType.Minus => -(double)(right ?? 0),
                TokenType.Bang => !IsTruthy(right),
                _ => null
            };
    }

    private object? Evaluate(Binary binary)
    {
        var left = this.Evaluate(binary.Left);
        var right = this.Evaluate(binary.Right);

        switch (binary.Operator.Type)
        {
            case TokenType.Greater:
            case TokenType.GreaterEqual:
            case TokenType.Less:
            case TokenType.LessEqual:
            case TokenType.Minus:
            case TokenType.Slash:
            case TokenType.Star:
                if (left is not double || right is not double)
                    throw new RuntimeException(binary.Operator, "Operator requires numbers");
                break;
        }

        return binary.Operator.Type switch
        {
            TokenType.Greater => (double)(left ?? 0) > (double)(right ?? 0),
            TokenType.GreaterEqual => (double)(left ?? 0) >= (double)(right ?? 0),
            TokenType.Less => (double)(left ?? 0) < (double)(right ?? 0),
            TokenType.LessEqual => (double)(left ?? 0) <= (double)(right ?? 0),
            TokenType.BangEqual => !IsEqual(left, right),
            TokenType.EqualEqual => IsEqual(left, right),
            TokenType.Minus => (double)(left ?? 0) - (double)(right ?? 0),
            TokenType.Slash => (double)(left ?? 0) / (double)(right ?? 0),
            TokenType.Star => (double)(left ?? 0) * (double)(right ?? 0),
            TokenType.Plus when left is string s1 => s1 + right?.ToString(),
            TokenType.Plus when right is string s1 => left?.ToString() + s1,
            TokenType.Plus when left is double d1 && right is double d2 => d1 + d2,
            TokenType.Plus => throw new RuntimeException(binary.Operator, "Plus operator can only be used on two numbers or two strings"),
            TokenType.Comma => right,
            _ => null
        }; ;
    }

    private object? Evaluate(Ternary ternary) =>
        IsTruthy(this.Evaluate(ternary.Test)) ? this.Evaluate(ternary.Left) : this.Evaluate(ternary.Right);

    private static bool IsTruthy(object? value) => value != null && (value is not bool b || b);

    private static bool IsEqual(object? a, object? b) =>
        a == null ? b == null : a.Equals(b);

    private static string Stringify(object? obj)
    {
        if (obj is null)
            return "nil";

        if (obj is double d)
        {
            var text = d.ToString();
            if (text.EndsWith(".0"))
                text = text[..^3];
            return text;
        }

        return obj.ToString()!;
    }
}
