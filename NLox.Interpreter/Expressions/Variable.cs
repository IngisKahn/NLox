namespace NLox.Interpreter.Expressions;

public record Variable(Token Name) : IExpression;