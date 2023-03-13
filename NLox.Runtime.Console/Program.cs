using NLox.Runtime;
using System.Text;
using static NLox.Runtime.Common;
using VirtualMachine vm = new();

if (args.Length == 1)
    await Repl();
else if (args.Length == 2)
    await RunFile(args[1]);
else
{
    Console.WriteLine("Usage: NLox [path]");
    Environment.Exit(64);
}

async Task Repl()
{
    var reader = Console.In;

    for (; ; )
    {
        Console.Write("> ");
        var line = await reader.ReadLineAsync();
        if (line == null)
        {
            Console.WriteLine();
            break;
        }
        try
        {
            Interpret(line);
        }
        catch (Exception e) 
        {
            Console.WriteLine(e.Message);
        }
    }
}

async Task RunFile(string path)
{
    var source = await File.ReadAllTextAsync(path);
    try
    {
        Interpret(source);
    }
    catch (RuntimeException)
    {
        Environment.Exit(70);
    }
    catch (CompileException)
    {
        Environment.Exit(65);
    }

}

