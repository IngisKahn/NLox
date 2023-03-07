namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record WhileStatement(IExpression Condition, IStatement Body) : IStatement;
