namespace NLox.Interpreter.Statements;
public record Block(IList<IStatement> Statements) : IStatement;
