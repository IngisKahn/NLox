namespace NLox.Interpreter;

using System.Collections.Generic;

public record Instance(ClassCallable Class)
{
    private readonly Dictionary<string, object?> fields = new();

    public object? this[Token name] => this.fields.TryGetValue(name.Lexeme, out var value) ? value : throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");
}
