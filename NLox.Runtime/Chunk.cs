namespace NLox.Runtime;
public sealed class Chunk : IDisposable
{
    private readonly Vector<byte> code = new();

    public int Count => this.code.Count;

    public Vector<Value> Constants { get; } = new();

    public void Write(byte b) => this.code.Write(b);
    public byte this[int index] => this.code[index];
    public int AddConstant(Value value)
    {
        this.Constants.Write(value);
        return this.Constants.Count - 1;
    }

    public void Dispose()
    {
        this.code.Dispose();
        this.Constants.Dispose();
    }
}
