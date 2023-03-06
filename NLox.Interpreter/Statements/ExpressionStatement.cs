namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record ExpressionStatement(IExpression Expression) : IStatement;
