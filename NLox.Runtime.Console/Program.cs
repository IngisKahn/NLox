using NLox.Runtime;
using static NLox.Runtime.Common;
using VirtualMachine vm = new();
using var chunk = new Chunk();
var constant = chunk.AddConstant(1.2);
chunk.Write((byte)OpCode.Constant, 123);
chunk.Write((byte)constant, 123);
chunk.Write((byte)OpCode.Negate, 123);
chunk.Write((byte)OpCode.Return, 123);

DisassembleChunk(chunk, "test chunk");

vm.Interpret(chunk);

