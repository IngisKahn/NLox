namespace NLox.Interpreter;

public record Grouping(IExpression Expression) : IExpression;
