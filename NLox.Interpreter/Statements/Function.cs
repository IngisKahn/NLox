namespace NLox.Interpreter.Statements;

public record Function(Token Name, IList<Token> Parameters, Block Body) : IStatement;
