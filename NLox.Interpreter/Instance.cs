﻿namespace NLox.Interpreter;

using System.Collections.Generic;

public record Instance(ClassCallable Class)
{
    private readonly Dictionary<string, object?> fields = new();

    public object? this[Token name]
    {
        get => this.fields.TryGetValue(name.Lexeme, out var value) ? value : (this.Class.FindMethod(name.Lexeme) ?? throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.")).Bind(this);
        set => this.fields[name.Lexeme] = value;
    }
}
