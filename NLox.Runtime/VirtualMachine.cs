namespace NLox.Runtime;

using System.Runtime.InteropServices;

public unsafe class VirtualMachine : IDisposable
{
    private const int stackMax = 256;

    private Chunk? chunk;
    private byte* ip;
    private readonly Value* stack;
    private Value* stackTop;
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
            this.chunk?.Dispose();
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

    public InterpretResult InterpretResult(Chunk chunk)
    {
        this.chunk = chunk;
        this.ip = chunk.Code.Data;
        return this.Run();
    }

    private byte ReadByte() => *this.ip++;
    private Value ReadConstant() => this.chunk.Constants[this.ReadByte()];

    private InterpretResult Run()
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
            Common.DisassembleInstruction(this.chunk, (int)(this.ip - this.chunk.Code.Data));
#endif
            var instruction = (OpCode)this.ReadByte();
            switch (instruction)
            {
                case OpCode.Constant:
                    var constant = this.ReadConstant();
                    this.Push(constant);
                    break;
                case OpCode.Return:
                    Common.PrintValue(this.Pop());
                    Console.WriteLine();
                    return Runtime.InterpretResult.Ok;
                default:
                    return Runtime.InterpretResult.RuntimeError;
            }
        }
    }
}

public enum InterpretResult
{
    Ok,
    CompileError,
    RuntimeError
}
