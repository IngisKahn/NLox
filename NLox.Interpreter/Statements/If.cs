namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record If(IExpression Condition, IStatement Then, IStatement? Else = null) : IStatement;