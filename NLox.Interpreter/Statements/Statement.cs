namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public abstract record Statement(IExpression Expression);

public record ExpressionStatement(IExpression Expression) : Statement(Expression);
public record PrintStatement(IExpression Expression) : Statement(Expression);
