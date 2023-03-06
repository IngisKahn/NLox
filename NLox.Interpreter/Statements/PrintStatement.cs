namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record PrintStatement(IExpression Expression) : IStatement;
