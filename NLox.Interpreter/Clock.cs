namespace NLox.Interpreter;

public class Clock : ICallable
{
    public int Arity => 0;

    public object? Call(Interpreter interpreter, IList<object> arguments) => DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;

    public override string ToString() => "<native fn>";
}