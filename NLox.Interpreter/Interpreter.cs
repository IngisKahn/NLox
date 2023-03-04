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
            TokenType.Minus => (double)(left ?? 0) - (double)(right ?? 0),
            TokenType.Slash => (double)(left ?? 0) / (double)(right ?? 0),
            TokenType.Star => (double)(left ?? 0) * (double)(right ?? 0),
            TokenType.Plus when left is double d1 && right is double d2 => d1 + d2,
            TokenType.Plus when left is string s1 && right is string s2 => s1 + s2,
            _ => null
        };
    }

    private static bool IsTruthy(object? value) => value != null && (value is not bool b || b);
}
