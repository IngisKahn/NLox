namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public record VarStatement(Token Name, IExpression? Expression) : Statement(Expression);
