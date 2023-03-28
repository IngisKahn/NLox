#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

using System.Runtime.InteropServices;


public unsafe class VirtualMachine : IDisposable
{
    private const int stackMax = 256;

    private byte* ip;
    private readonly Value* stack;
    private Value* stackTop;
    private Object* objects;
    private readonly Table strings = new();
    private readonly Table globals = new();

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
        this.globals.Free();
        this.strings.Free();
        NativeMemory.Free(this.stack);
        this.FreeObjects();
        disposedValue = true;
    }

    private void FreeObjects()
    {
        var o = this.objects;
        while (o != null)
        {
            var next = o->Next;
            Object.Free(o);
            o = next;
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

    public void Interpret(string source)
    {
        using Chunk chunk = new();
        if (!new Compiler(source, chunk, RegisterObject, strings).Compile())
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
                case OpCode.Equal:
                    {
                        var b = this.Pop();
                        this.Push(this.Pop().Equals(b));
                    }
                    break;
                case OpCode.Add:
                    if (this.Peek(0)->IsString && this.Peek(1)->IsString)
                        this.Concatenate();
                    else if (this.Peek(0)->IsNumber && this.Peek(1)->IsNumber)
                    {
                        double b = this.Pop();
                        *this.Peek(0) = *this.Peek(0) + b;
                    }
                    else
                    {
                        RuntimeError(chunk, "Operands must be two numbers or two strings.");
                        return;
                    }
                    break;
                case OpCode.Greater:
                case OpCode.Less:
                case OpCode.Subtract:
                case OpCode.Multiply:
                case OpCode.Divide:
                    this.BinaryOp(chunk, instruction);
                    break;
                case OpCode.Not:
                    *this.Peek(0) = IsFalsey(*this.Peek(0));
                    break;
                case OpCode.False:
                    this.Push(false);
                    break;
                case OpCode.Nil:
                    this.Push(new());
                    break;
                case OpCode.True:
                    this.Push(true);
                    break;
                case OpCode.Negate:
                    if (!this.Peek(0)->IsNumber)
                    {
                        this.RuntimeError(chunk, "Operand must be a number.");
                        throw new RuntimeException();
                    }
                    *this.Peek(0) = -*this.Peek(0);
                    break;
                case OpCode.Pop:
                    this.Pop();
                    break;
                case OpCode.GetGlobal:
                    {
                        ObjectString* name = this.ReadConstant(chunk);
                        var value = this.globals.Get(name);
                        if (value == null)
                            throw new RuntimeException($"Undefined variable '{name->ToString()}'.");
                        this.Push(*value);
                        break;
                    }
                case OpCode.DefineGlobal:
                    {
                        ObjectString* name = this.ReadConstant(chunk);
                        this.globals.Add(name, *Peek(0));
                        Pop();
                        break;
                    }
                case OpCode.SetGlobal:
                    {
                        ObjectString* name = this.ReadConstant(chunk);
                        if (this.globals.Add(name, *this.Peek(0)))
                        {
                            this.globals.Delete(name);
                            throw new RuntimeException($"Undefined variable '{name->ToString()}'.");
                        }
                    }
                    break;
                case OpCode.Print:
                    Common.PrintValue(this.Pop());
                    Console.WriteLine();
                    break;
                case OpCode.Return:
                    return;
                default:
                    throw new RuntimeException("Invalid opcode, corrupt program");
            }
        }
    }

    private void Concatenate()
    {
        ObjectString* b = this.Pop();
        ObjectString* a = *this.Peek(0);

        var length = a->Length + b->Length;
        var chars = Memory.Allocate<byte>((nuint)length);
        Buffer.MemoryCopy(a->Chars, chars, (nuint)a->Length, (nuint)a->Length);
        Buffer.MemoryCopy(b->Chars, chars + a->Length, (nuint)b->Length, (nuint)b->Length);
        *this.Peek(0) = ObjectString.TakeString(chars, length, RegisterObject, strings);
    }

    public void RegisterObject(IntPtr p)
    {
        var o = (Object*)p;
        o->Next = this.objects;
        this.objects = o;
    }


    private static bool IsFalsey(Value value) => value.IsNil || value.IsBool && !value;

    private void RuntimeError(Chunk chunk, string message)
    {
        Console.Error.WriteLine(message);
        var instruction = this.ip - chunk.Code.Data - 1;
        var line = chunk.Lines[(int)instruction];

        Console.Error.WriteLine($"[line {line}] in script");
        this.ResetStack();
    }

    private Value* Peek(int distance) => this.stackTop - (distance + 1);

    private void BinaryOp(Chunk chunk, OpCode opCode)
    {
        if (!this.Peek(0)->IsNumber || this.Peek(1)->IsNumber)
        {
            this.RuntimeError(chunk, "Operands must be numbers.");
            throw new RuntimeException();
        }

        var b = this.Pop();
        var a = *this.Peek(0);

        *this.Peek(0) = opCode switch
        {
            OpCode.Greater => (double)a > b,
            OpCode.Less => (double)a < b,
            OpCode.Add => a + b,
            OpCode.Subtract => a - b,
            OpCode.Multiply => a * b,
            OpCode.Divide => a / b,
            _ => throw new InvalidOperationException()
        };
    }
}
