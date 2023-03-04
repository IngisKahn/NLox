﻿using System.Text;

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
        await Run(line);
        hadError = false;
    }
}

async Task Run(string source)
{
    Scanner scanner = new(source, Error);
    var tokens = await scanner.ScanTokens();
    Parser parser = new(tokens, TokenError);
    var expression = await parser.Parse(); 
    
    // Stop if there was a syntax error.
    if (hadError || expression == null) 
        return;

    Console.WriteLine(AstPrinter.Print(expression));
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