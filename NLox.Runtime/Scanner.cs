#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public class Scanner
{
    private readonly string source;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public Scanner(string source) => this.source = source;

    public Token ScanToken() { }
}
