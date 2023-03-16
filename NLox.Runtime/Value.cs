namespace NLox.Runtime;
using System.Runtime.InteropServices;

public enum ValueType : byte
{
    Bool,
    Nil,
    Number
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct Value : IEquatable<Value>
{
    [FieldOffset(8)]
    private readonly ValueType type;

    public ValueType Type => this.type;

    [FieldOffset(0)]
    private readonly double doubleValue;

    [FieldOffset(0)]
    private readonly bool boolValue;

    public bool IsBool => this.type == ValueType.Bool;
    public bool IsNil => this.type == ValueType.Nil;
    public bool IsNumber => this.type == ValueType.Number;

    public Value() => this.type = ValueType.Nil;

    private Value(double d)
    {
        this.doubleValue = d;
        this.type = ValueType.Number;
    }

    private Value(bool b)
    {
        this.boolValue = b;
        this.type = ValueType.Bool;
    }

    public static implicit operator double(Value v) => v.doubleValue;
    public static implicit operator Value(double d) => new(d);
    public static implicit operator bool(Value v) => v.boolValue;
    public static implicit operator Value(bool b) => new(b);

    public bool Equals(Value other) =>
        type == other.type && this.type switch {
            ValueType.Bool => this.boolValue == other.boolValue,
            ValueType.Number => Math.Abs(this.doubleValue - other.doubleValue) < 0.00000001,
            _ => true
        };

    public override bool Equals(object? obj)
    {
        return obj is Value other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int) type;
            hashCode = (hashCode * 397) ^ doubleValue.GetHashCode();
            hashCode = (hashCode * 397) ^ boolValue.GetHashCode();
            return hashCode;
        }
    }
}
