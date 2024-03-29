﻿using System.Runtime.CompilerServices;

namespace NLox.Runtime;
using System.Runtime.InteropServices;

public enum ValueType : byte
{
    Bool,
    Nil,
    Number,
    Object
}

public enum ObjectType : byte
{
    String
}

[StructLayout(LayoutKind.Explicit)]
public readonly unsafe struct Value : IEquatable<Value>
{
    public static readonly Value Nil = new();

    [FieldOffset(8)]
    private readonly ValueType type;

    public ValueType Type => this.type;

    [FieldOffset(0)]
    private readonly double doubleValue;

    [FieldOffset(0)]
    private readonly bool boolValue;
    [FieldOffset(0)]
    private readonly Object* objectValue;

    public bool IsBool => this.type == ValueType.Bool;
    public bool IsNil => this.type == ValueType.Nil;
    public bool IsNumber => this.type == ValueType.Number;
    public bool IsObject => this.type == ValueType.Object;
    public bool IsObjectType(ObjectType type) => this.IsObject && this.objectValue->Type == type;
    public bool IsString => this.IsObjectType(ObjectType.String);

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

    private Value(Object* o)
    {
        this.objectValue = o;
        this.type = ValueType.Object;
    }

    public static implicit operator double(Value v) => v.doubleValue;
    public static implicit operator Value(double d) => new(d);
    public static implicit operator bool(Value v) => v.boolValue;
    public static implicit operator Value(bool b) => new(b);
    public static implicit operator Object*(Value v) => v.objectValue;
    public static implicit operator Value(Object* o) => new(o);
    public static implicit operator ObjectString*(Value v) => (ObjectString*)v.objectValue;
    public static implicit operator Value(ObjectString* o) => new((Object*)o);

    public bool Equals(Value other)
    {
        if (this.type != other.type)
            return false;
        return this.type switch
        {
            ValueType.Bool => this.boolValue == other.boolValue,
            ValueType.Number => Math.Abs(this.doubleValue - other.doubleValue) < 0.00000001,
            ValueType.Object => this.objectValue == other.objectValue,
            _ => true,
        };
    }

    public override bool Equals(object? obj) => obj is Value other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(type, (nuint)objectValue);
}