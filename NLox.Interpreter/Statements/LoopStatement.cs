namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record LoopStatement(IStatement? Initializer, IExpression? Condition, IExpression? Increment, IStatement Body) : IStatement;
