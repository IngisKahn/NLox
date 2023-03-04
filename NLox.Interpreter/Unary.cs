namespace NLox.Interpreter;

public record Unary(Token Operator, IExpression Right) : IExpression;
