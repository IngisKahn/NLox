namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record Return(Token keyword, IExpression? Value) : IStatement;