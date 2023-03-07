namespace NLox.Interpreter;
public interface ICallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, IList<object> arguments);
}
