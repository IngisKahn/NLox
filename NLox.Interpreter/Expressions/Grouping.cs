namespace NLox.Interpreter.Expressions;

public record Grouping(IExpression Expression) : IExpression;
