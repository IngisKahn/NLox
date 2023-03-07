namespace NLox.Interpreter.Expressions;

public record Logical(IExpression Left, Token Operator, IExpression Right) : IExpression;