namespace NLox.Interpreter.Expressions;

public record Binary(IExpression Left, Token Operator, IExpression Right) : IExpression;
