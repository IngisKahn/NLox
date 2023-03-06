﻿namespace NLox.Interpreter;

using System;

public class Scope
{
    private readonly Dictionary<string, object?> values = new();
    private readonly Scope? enclosing;

    public Scope(Scope? enclosing = null) => this.enclosing = enclosing;

    public void Define(string name, object? value) => this.values[name] = value;

    public object? this[Token name] => 
        this.values.TryGetValue(name.Lexeme, out var value) 
            ? value 
            : enclosing != null 
                ? enclosing[name] 
                : throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");

    public void Assign(Token name, object? value)
    {
        if (this.values.ContainsKey(name.Lexeme))
            this.values[name.Lexeme] = value;
        else if (this.enclosing != null)
            this.enclosing.Assign(name, value);
        else
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}