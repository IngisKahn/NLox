namespace NLox.Interpreter;

public class Scanner
{
    private readonly string source;

    public Scanner(string source) => this.source = source;

    public IEnumerable<Token> ScanTokens() => throw new NotImplementedException();
}
