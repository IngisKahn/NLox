using System.Text;

using NLox.Interpreter;

var hadError = false;

if (args.Length > 1)
{
    Console.WriteLine("Usage: NLox [script]");
    Environment.Exit(64);
}
if (args.Length == 1)
    await RunFile(args[0]);
else
    await RunPrompt();

async Task RunFile(string path)
{
    var bytes = await File.ReadAllBytesAsync(path);
    Run(Encoding.Default.GetString(bytes));
    if (hadError)
        Environment.Exit(65);
}

async Task RunPrompt()
{
    var reader = Console.In;

    for (; ; )
    {
        Console.Write("> ");
        var line = await reader.ReadLineAsync();
        if (line == null)
            break;
        Run(line);
        hadError = false;
    }
}

void Run(string source)
{
    Scanner scanner = new(source);
    var tokens = scanner.ScanTokens();

    // For now, just print the tokens.
    foreach (var token in tokens)
        Console.WriteLine(token);
}

Task Error(int line, string message) => Report(line, "", message);

Task Report(int line, string where, string message)
{
    hadError = true;
    return Console.Error.WriteLineAsync($"[line {line}] Error{where}: {message}");
}