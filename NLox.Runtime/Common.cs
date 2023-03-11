namespace NLox.Runtime;

public static class Common
{
    public static void DisassembleChunk(Chunk chunk, string name)
    {
        Console.WriteLine($"== {name} ==");

        for (var offset = 0; offset < chunk.Count;)
            offset = DisassembleInstruction(chunk, offset);
    }

    public static int DisassembleInstruction(Chunk chunk, int offset)
    {
        Console.Write($"{offset:0000} ");
        if (offset > 0 &&
          chunk.Lines[offset] == chunk.Lines[offset - 1])
            Console.Write("   | ");
        else
            Console.Write($"{chunk.Lines[offset],4} ");

        var instruction = (OpCode)chunk[offset];
        switch (instruction)
        {
            case OpCode.Constant:
                return ConstantInstruction(instruction.ToString(), chunk, offset);
            case OpCode.Negate:
                return SimpleInstruction(instruction.ToString(), offset);
            case OpCode.Return:
                return SimpleInstruction(instruction.ToString(), offset);
            default:
                Console.WriteLine("Unknown opcode " + instruction);
                return offset + 1;
        }
    }

    public static int SimpleInstruction(string name, int offset)
    {
        Console.WriteLine(name);
        return offset + 1;
    }

    public static int ConstantInstruction(string name, Chunk chunk, int offset)
    {
        var constant = chunk[offset + 1];
        Console.Write($"{name,-16} {constant,4} '");
        PrintValue(chunk.Constants[constant]);
        Console.WriteLine('\'');
        return offset + 2;
    }

    public static void PrintValue(Value value) => Console.Write(value);

}
