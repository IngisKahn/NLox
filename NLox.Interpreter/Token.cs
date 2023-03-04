namespace NLox.Interpreter;

public record Token(TokenType Type, string Lexeme, object? Literal, int Line);