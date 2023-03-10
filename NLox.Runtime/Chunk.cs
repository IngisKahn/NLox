namespace NLox.Runtime;

using System.Runtime.InteropServices;

public sealed unsafe class Chunk : IDisposable
{
    public int Count { get; private set; }
    public nuint Capacity { get; private set; }
    private byte* code;

    public void Write(byte b)
    {
        if (this.Capacity < (nuint)this.Count + 1)
        {
            var oldCapacity = this.Capacity;
            this.Capacity = Memory.GrowCapacity(oldCapacity);
            this.code = Memory.GrowArray(this.code, oldCapacity, this.Capacity);
        }
        this.code[this.Count++] = b;
    }

    public byte this[int index] => this.code[index];

    public void Free()
    {
        Memory.FreeArray(this.code, this.Capacity);
        this.code = null;
        this.Count = (int)(this.Capacity = 0);
    }

    public void Dispose()
    {
        if (this.code != null)
            NativeMemory.Free(this.code);
    }
}
