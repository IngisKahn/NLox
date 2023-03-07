namespace NLox.Interpreter;

using NLox.Interpreter.Statements;

public class CallableFunction : ICallable
{
    private readonly Function declaration;
    public int Arity => this.declaration.Parameters.Count;

    public CallableFunction(Function declaration) => this.declaration = declaration;

    public object? Call(Interpreter interpreter, IList<object> arguments)
    {
        Scope scope = new(interpreter.Scope);
        for (var i = 0; i < this.declaration.Parameters.Count; i++)
            scope.Define(this.declaration.Parameters[i].Lexeme, arguments[i]);

        interpreter.ExecuteBlock(this.declaration.Body, scope);

        return null;
    }

    public override string ToString() => $"<fn {this.declaration.Name.Lexeme}>";
}
