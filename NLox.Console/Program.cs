using System.Text;

if (args.Length > 1)
{
    Console.WriteLine("Usage: NLox [script]");
    Environment.Exit(64);
}
if (args.Length == 1)
    await RunFile(args[0]);
else
    await RunPrompt();

static async Task RunFile(string path)
{
    var bytes = await File.ReadAllBytesAsync(path);
    Run(Encoding.Default.GetString(bytes));
}

static async Task RunPrompt()
{
    var reader = Console.In;

    for (; ; )
    {
        Console.Write("> ");
        var line = await reader.ReadLineAsync();
        if (line == null)
            break;
        Run(line);
    }
}

static void Run(string source)
{
    Scanner scanner = new(source);
    var tokens = scanner.ScanTokens();

    // For now, just print the tokens.
    foreach (var token in tokens)
        Console.WriteLine(token);
}

internal class Scanner
{
    private readonly string source;

    public Scanner(string source) => this.source = source;

    public IEnumerable<string> ScanTokens() => throw new NotImplementedException();
}