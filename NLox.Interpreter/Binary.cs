namespace NLox.Interpreter;

public record Binary(IExpression Left, Token Operator, IExpression Right) : IExpression;
