namespace NLox.Runtime;

using System.Runtime.InteropServices;

public sealed unsafe class Vector<T> : IDisposable where T : unmanaged
{
    public int Count { get; private set; }
    public nuint Capacity { get; private set; }
    public unsafe T* Data { get; private set; }
    private bool disposedValue;

    public void Write(T b)
    {
        if (this.Capacity < (nuint)this.Count + 1)
        {
            var oldCapacity = this.Capacity;
            this.Capacity = Memory.GrowCapacity(oldCapacity);
            this.Data = Memory.GrowArray(this.Data, oldCapacity, this.Capacity);
        }
        this.Data[this.Count++] = b;
    }

    public T this[int index] => this.Data[index];

    public void Free()
    {
        Memory.FreeArray(this.Data, this.Capacity);
        this.Data = null;
        this.Count = (int)(this.Capacity = 0);
    }
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {

            }

            if (this.Data != null)
                NativeMemory.Free(this.Data);

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
