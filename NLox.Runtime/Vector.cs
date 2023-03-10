namespace NLox.Runtime;

using System.Runtime.InteropServices;

public sealed unsafe class Vector<T> : IDisposable where T : unmanaged
{
    public int Count { get; private set; }
    public nuint Capacity { get; private set; }
    private T* code;
    private bool disposedValue;

    public void Write(T b)
    {
        if (this.Capacity < (nuint)this.Count + 1)
        {
            var oldCapacity = this.Capacity;
            this.Capacity = Memory.GrowCapacity(oldCapacity);
            this.code = Memory.GrowArray(this.code, oldCapacity, this.Capacity);
        }
        this.code[this.Count++] = b;
    }

    public T this[int index] => this.code[index];

    public void Free()
    {
        Memory.FreeArray(this.code, this.Capacity);
        this.code = null;
        this.Count = (int)(this.Capacity = 0);
    }
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {

            }

            if (this.code != null)
                NativeMemory.Free(this.code);

            disposedValue = true;
        }
    }

    ~Vector() => this.Dispose(disposing: false);

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
