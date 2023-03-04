namespace NLox.Interpreter.Expressions;

public record Literal(object? Value) : IExpression;
