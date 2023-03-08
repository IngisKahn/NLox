namespace NLox.Interpreter.Expressions;

public record Assign(Token Name, IExpression Value) : IExpression;
