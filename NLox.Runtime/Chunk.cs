namespace NLox.Runtime;
public sealed class Chunk : IDisposable
{
    private readonly Vector<byte> code = new();
    public Vector<int> Lines { get; } = new(); // replace with compressed rank

    public int Count => this.code.Count;

    public Vector<Value> Constants { get; } = new();

    public void Write(byte b, int line)
    {
        this.code.Write(b);
        this.Lines.Write(line);
    }
    public byte this[int index] => this.code[index];
    public int AddConstant(Value value)
    {
        this.Constants.Write(value);
        return this.Constants.Count - 1;
    }

    public void Dispose()
    {
        this.code.Dispose();
        this.Lines.Dispose();
        this.Constants.Dispose();
    }
}
