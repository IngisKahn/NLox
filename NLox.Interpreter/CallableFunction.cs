namespace NLox.Interpreter;

using NLox.Interpreter.Statements;

public class CallableFunction : ICallable
{
    private readonly Function declaration;
    private readonly Scope closure;
    private readonly bool isInitializer;
    public int Arity => this.declaration.Parameters.Count;

    public CallableFunction(Function declaration, Scope closure, bool isInitializer)
    {
        this.declaration = declaration;
        this.closure = closure;
        this.isInitializer = isInitializer;
    }

    public object? Call(Interpreter interpreter, IList<object> arguments)
    {
        Scope scope = new(this.closure);
        for (var i = 0; i < this.declaration.Parameters.Count; i++)
            scope.Define(this.declaration.Parameters[i].Lexeme, arguments[i]);
        interpreter.ReturnValue = null;
        interpreter.ExecuteBlock(this.declaration.Body, scope);
        
        return this.isInitializer ? this.closure.GetAt(0, "this") : interpreter.ReturnValue;
    }

    public override string ToString() => $"<fn {this.declaration.Name.Lexeme}>";

    public CallableFunction Bind(Instance instance)
    {
        Scope scope = new(this.closure);
        scope.Define("this", instance);
        return new(this.declaration, scope, this.isInitializer);
    }
}
