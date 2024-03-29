﻿using System.Text;

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
            case OpCode.GetLocal:
            case OpCode.SetLocal:
                return ByteInstruction(instruction.ToString(), chunk, offset);
            case OpCode.Constant:
            case OpCode.DefineGlobal:
            case OpCode.GetGlobal:
            case OpCode.SetGlobal:
                return ConstantInstruction(instruction.ToString(), chunk, offset);
            case OpCode.Greater:
            case OpCode.Less:
            case OpCode.Equal:
            case OpCode.False:
            case OpCode.True:
            case OpCode.Nil:
            case OpCode.Not:
            case OpCode.Add:
            case OpCode.Subtract:
            case OpCode.Multiply:
            case OpCode.Divide:
            case OpCode.Negate:
            case OpCode.Pop:
            case OpCode.Print:
            case OpCode.Return:
                return SimpleInstruction(instruction.ToString(), offset);
            default:
                Console.WriteLine("Unknown opcode " + instruction);
                return offset + 1;
        }
    }

    public static int ByteInstruction(string name, Chunk chunk, int offset)
    {
        var slot = chunk.Code[offset + 1];
        Console.WriteLine($"{name,-16} {slot,4} '");
        return offset + 2;
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
        Console.WriteLine('\''); Span<byte> a = new Span<byte>();
        a.SequenceEqual(a);
        return offset + 2;
    }

    public static unsafe void PrintValue(Value value) =>
        Console.Write(value.Type switch
        {
            ValueType.Bool => value ? "true" : "false",
            ValueType.Number => (double)value,
            ValueType.Object => ObjectToString(value),
            _ => "nil"
        });

    public static unsafe string ObjectToString(Object* value) =>
        value->Type switch
        {
            ObjectType.String => Encoding.ASCII.GetString(((ObjectString*)value)->Chars, ((ObjectString*)value)->Length),
            _ => "[ERROR]"
        };
}
