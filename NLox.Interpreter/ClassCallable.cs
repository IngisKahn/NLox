namespace NLox.Interpreter;

using System.Collections.Generic;

public class ClassCallable : ICallable
{
    private readonly Dictionary<string, CallableFunction> methods;
    public string Name { get; }

    public int Arity => this.FindMethod("init")?.Arity ?? 0;

    public ClassCallable(string name, Dictionary<string, CallableFunction> methods)
    {
        this.Name = name; 
        this.methods = methods; 
    }

    public override string ToString() => this.Name;
    public object? Call(Interpreter interpreter, IList<object> arguments)
    {
        Instance instance = new(this);

        this.FindMethod("init")?.Bind(instance).Call(interpreter, arguments);

        return instance;
    }

    public CallableFunction? FindMethod(string name)
    {
        if (this.methods.TryGetValue(name, out var method)) 
            return method;
        return null;
    }
}
