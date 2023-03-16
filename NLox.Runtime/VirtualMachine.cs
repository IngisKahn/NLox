#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

using System.Runtime.InteropServices;


public unsafe class VirtualMachine : IDisposable
{
    private const int stackMax = 256;

    private byte* ip;
    private readonly Value* stack;
    private Value* stackTop;

    private readonly Compiler compiler = new();
    private bool disposedValue;

    public VirtualMachine()
    {
        this.stack = (Value*)NativeMemory.Alloc(VirtualMachine.stackMax, (nuint)sizeof(Value));
        this.ResetStack();
    }

    private void ResetStack() => this.stackTop = this.stack;

    private void Push(Value value) => *this.stackTop++ = value;

    private Value Pop() => *--this.stackTop;

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
            return;
        if (disposing)
        {

        }
        NativeMemory.Free(this.stack);
        disposedValue = true;
    }

    ~VirtualMachine()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Interpret(string source)
    {
        using Chunk chunk = new();
        if (!this.compiler.Compile(source, chunk))
            throw new CompileException();

        this.Interpret(chunk);
    }

    public void Interpret(Chunk chunk)
    {
        this.ip = chunk.Code.Data;
        this.Run(chunk);
    }

    private byte ReadByte() => *this.ip++;
    private Value ReadConstant(Chunk chunk) => chunk.Constants[this.ReadByte()];

    private void Run(Chunk chunk)
    {
        for (; ; )
        {
#if DEBUG_TRACE_EXECUTION
            Console.Write("          ");
            for (var slot = this.stack; slot < this.stackTop; slot++)
            {
                Console.Write("[ ");
                Common.PrintValue(*slot);
                Console.Write(" ]");
            }
            Console.WriteLine();
            Common.DisassembleInstruction(chunk, (int)(this.ip - chunk.Code.Data));
#endif
            var instruction = (OpCode)this.ReadByte();
            switch (instruction)
            {
                case OpCode.Constant:
                    var constant = this.ReadConstant(chunk);
                    this.Push(constant);
                    break;
                case OpCode.Add:
                case OpCode.Subtract:
                case OpCode.Multiply:
                case OpCode.Divide:
                    this.BinaryOp(instruction);
                    break;
                case OpCode.Negate:
                    this.stackTop[-1] = -this.stackTop[-1];
                    break;
                case OpCode.Return:
                    Common.PrintValue(this.Pop());
                    Console.WriteLine();
                    return;
                default:
                    throw new RuntimeException("Invalid opcode, corrupt program");
            }
        }
    }

    private void BinaryOp(OpCode opCode)
    {
        var b = this.Pop();
        var a = this.stackTop[-1];

        this.stackTop[-1] = opCode switch
        {
            OpCode.Add => a + b,
            OpCode.Subtract => a - b,
            OpCode.Multiply => a * b,
            OpCode.Divide => a / b,
            _ => throw new InvalidOperationException()
        };
    }
}
