namespace NLox.Runtime;
public sealed class Chunk : IDisposable
{
    public Vector<int> Lines { get; } = new(); // replace with compressed rank

    public int Count => this.Code.Count;

    public Vector<Value> Constants { get; } = new();

    public Vector<byte> Code { get; } = new();

    public void Write(byte b, int line)
    {
        this.Code.Write(b);
        this.Lines.Write(line);
    }
    public byte this[int index] => this.Code[index];
    public int AddConstant(Value value)
    {
        this.Constants.Write(value);
        return this.Constants.Count - 1;
    }

    public void Dispose()
    {
        this.Code.Dispose();
        this.Lines.Dispose();
        this.Constants.Dispose();
    }
}
