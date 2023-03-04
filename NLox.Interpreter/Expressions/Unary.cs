namespace NLox.Interpreter.Expressions;

public record Unary(Token Operator, IExpression Right) : IExpression;
