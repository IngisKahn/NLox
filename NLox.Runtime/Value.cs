namespace NLox.Runtime;

public struct Value
{
    private double value;
    public Value(double d) => this.value = d;
    public static implicit operator double(Value v) => v.value;
    public static implicit operator Value(double d) => new(d);
}
