#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public record Token(TokenType Type, string Source, int Start, int Length, int Line)
{
    public string Lexeme => this.Source[this.Start..(this.Start + this.Length)];
}
