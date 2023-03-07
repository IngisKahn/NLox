namespace NLox.Interpreter.Statements;
public record Block(List<IStatement> Statements) : IStatement;
