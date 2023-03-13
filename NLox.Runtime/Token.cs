#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public record Token(TokenType Type, int Start, int Length, int Line);
