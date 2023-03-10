using NLox.Runtime;
using System.Runtime.ExceptionServices;

using var chunk = new Chunk();
chunk.Write((byte)OpCode.Return);

DisassembleChunk(chunk, "test chunk");

void DisassembleChunk(Chunk chunk, string name)
{
    Console.WriteLine($"== {name} ==");

    for (var offset = 0; offset < chunk.Count;)
        offset = DisassembleInstruction(chunk, offset);
}

int DisassembleInstruction(Chunk chunk, int offset)
{
    Console.Write($"{offset:0000} ");

    var instruction = (OpCode)chunk[offset];
    switch (instruction)
    {
        case OpCode.Return:
            return SimpleInstruction(instruction.ToString(), offset);
        default:
            Console.WriteLine("Unknown opcode " + instruction);
            return offset + 1;
    }
}

int SimpleInstruction(string name, int offset)
{
    Console.WriteLine(name);
    return offset + 1;
}
