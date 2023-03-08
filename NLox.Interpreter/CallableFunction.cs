namespace NLox.Interpreter;

using NLox.Interpreter.Statements;

public class CallableFunction : ICallable
{
    private readonly Function declaration;
    private readonly Scope closure;
    public int Arity => this.declaration.Parameters.Count;

    public CallableFunction(Function declaration, Scope closure)
    {
        this.declaration = declaration;
        this.closure = closure;
    }

    public object? Call(Interpreter interpreter, IList<object> arguments)
    {
        Scope scope = new(this.closure);
        for (var i = 0; i < this.declaration.Parameters.Count; i++)
            scope.Define(this.declaration.Parameters[i].Lexeme, arguments[i]);
        interpreter.ReturnValue = null;
        interpreter.ExecuteBlock(this.declaration.Body, scope);

        return interpreter.ReturnValue;
    }

    public override string ToString() => $"<fn {this.declaration.Name.Lexeme}>";
}
