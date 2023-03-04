namespace NLox.Interpreter;

public record Literal(object Value) : IExpression;
