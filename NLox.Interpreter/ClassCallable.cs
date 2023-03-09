namespace NLox.Interpreter;

using System.Collections.Generic;

public class ClassCallable : ICallable
{
    public string Name { get; }

    public int Arity => 0;

    public ClassCallable(string name) => this.Name = name;

    public override string ToString() => this.Name;
    public object? Call(Interpreter interpreter, IList<object> arguments) =>
        new Instance(this);
}
