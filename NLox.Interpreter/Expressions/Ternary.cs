namespace NLox.Interpreter.Expressions;

public record Ternary(IExpression Test, IExpression Left, IExpression Right) : IExpression;
