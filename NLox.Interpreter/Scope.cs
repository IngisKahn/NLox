namespace NLox.Interpreter;

using System;

public class Scope
{
    private readonly Dictionary<string, object?> values = new();
    private readonly Scope? enclosing;

    public Scope? Enclosing => enclosing;

    public Scope(Scope? enclosing = null) => this.enclosing = enclosing;

    public void Define(string name, object? value) => this.values[name] = value;

    public object? this[Token name] => 
        this.values.TryGetValue(name.Lexeme, out var value) 
            ? value 
            : Enclosing != null 
                ? Enclosing[name] 
                : throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");

    public void Assign(Token name, object? value)
    {
        if (this.values.ContainsKey(name.Lexeme))
            this.values[name.Lexeme] = value;
        else if (this.Enclosing != null)
            this.Enclosing.Assign(name, value);
        else
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }

    private Scope Ancestor(int distance)
    {
        var scope = this;

        while (distance-- > 0)
            scope = scope.Enclosing!;

        return scope;
    }

    public object? GetAt(int distance, string name) => this.Ancestor(distance).values[name];

    public void AssignAt(int distance, Token name, object? value) => this.Ancestor(distance).values[name.Lexeme] = value;

}