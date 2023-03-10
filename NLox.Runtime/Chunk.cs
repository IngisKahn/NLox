namespace NLox.Runtime;

public class Chunk : IDisposable
{
    public int Count { get; private set; }
    public int Capacity { get; private set; }
    private unsafe byte* code;

    public void Write(byte b)
    {
        if (this.Capacity < this.Count + 1)
        {
            var oldCapacity = this.Capacity;
            this.Capacity = Memory.GrowCapacity(oldCapacity);

        }
    }

    public void Dispose() => throw new NotImplementedException();
}
