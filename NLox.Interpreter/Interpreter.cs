namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;

public class Interpreter
{
    public object? Evaluate(IExpression expression) => this.Evaluate((dynamic)expression);

    private object? Evaluate(Literal literal) => literal.Value;
    private object? Evaluate(Grouping grouping) => this.Evaluate(grouping.Expression);
    private object? Evaluate(Unary unary)
    {
        var right = this.Evaluate(unary.Right);
        return unary.Operator.Type switch
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
            TokenType.Plus when left is double d1 && right is double d2 => d1 + d2,
            TokenType.Plus when left is string s1 && right is string s2 => s1 + s2,
            TokenType.Comma => right,
            _ => null
        }; ;
    }

    private object? Evaluate(Ternary ternary) =>
        IsTruthy(this.Evaluate(ternary.Test)) ? this.Evaluate(ternary.Left) : this.Evaluate(ternary.Right);

    private static bool IsTruthy(object? value) => value != null && (value is not bool b || b);

    private static bool IsEqual(object? a, object? b) =>
        a == null ? b == null : a.Equals(b);
}
