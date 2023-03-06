using System.Text;

using NLox.Interpreter;
using NLox.Interpreter.Expressions;

//var expression = new Binary(
//new Unary(
//new Token(TokenType.Minus, "-", null, 1),
//new Literal(123)),
//new Token(TokenType.Star, "*", null, 1),
//new Grouping(
//new Literal(45.67)));

//Console.WriteLine(AstPrinter.Print(expression));

var hadError = false;

await Run("""
    var a = 1;
    var b = 2;
    print a + b;
    print a = 2;
    print a;
    """);

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
    await Run(Encoding.Default.GetString(bytes));
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
        try
        {
            await Run(line);
        }
        catch { }
        hadError = false;
    }
}

async Task Run(string source)
{
    Scanner scanner = new(source, Error);
    var tokens = await scanner.ScanTokens();
    Parser parser = new(tokens, TokenError);
    Interpreter interpreter = new();
    await foreach (var statement in parser.Parse())
    {
        // Stop if there was a syntax error.
        if (hadError)
            return;

        if (statement != null)
            interpreter.Interpret(statement);
    }
}

Task Error(int line, string message) => Report(line, "", message);

Task TokenError(Token token, string message) =>
    token.Type == TokenType.EoF
    ? Report(token.Line, " at end", message)
    : Report(token.Line, $" at '{token.Lexeme}'", message);

Task Report(int line, string where, string message)
{
    hadError = true;
    return Console.Error.WriteLineAsync($"[line {line}] Error{where}: {message}");
}
