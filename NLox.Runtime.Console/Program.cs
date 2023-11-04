using NLox.Runtime;
using VirtualMachine vm = new();

Chunk c = new();
c.Write((byte)OpCode.Constant, 123);
c.Write((byte)c.AddConstant(1.2), 123);
c.Write((byte)OpCode.Return, 123);
Common.DisassembleChunk(c, "test chunk");

return;

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
            vm.Interpret(line);
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
        vm.Interpret(source);
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

