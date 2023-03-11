namespace NLox.Runtime;

public unsafe class VirtualMachine : IDisposable
{
    private Chunk chunk;
    private byte* ip;
    private bool disposedValue;

    public VirtualMachine()
    { }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                this.chunk.Dispose();
            }
            disposedValue = true;
        }
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
            Common.DisassembleInstruction(this.chunk, (int)(this.ip - this.chunk.Code.Data));
#endif
            var instruction = (OpCode)this.ReadByte();
            switch (instruction) 
            {
                case OpCode.Constant:
                    var constant = this.ReadConstant();
                    VirtualMachine.PrintValue(constant);
                    Console.WriteLine();
                    break;
                case OpCode.Return:
                    return Runtime.InterpretResult.Ok;
                default:
                    return Runtime.InterpretResult.RuntimeError;
            }
        }
    }
    private static void PrintValue(Value value) => Console.Write(value);
}

public enum InterpretResult
{
    Ok,
    CompileError,
    RuntimeError
}
