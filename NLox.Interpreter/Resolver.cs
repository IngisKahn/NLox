namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;

public partial class Resolver
{
    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = 0;

    private enum FunctionType
    {
        None,
        Function
    }

    public Resolver(Interpreter interpreter) => this.interpreter = interpreter;

    private void BeginScope() => this.scopes.Push(new());

    private void EndScope() => scopes.Pop();

    private void Declare(Token name)
    {
        if (scopes.Count == 0)
            return;
        var scope = this.scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
            throw new RuntimeException(name, "Already a variable with this name in this scope.");
        scope[name.Lexeme] = false;
    }
    private void Define(Token name)
    {
        if (scopes.Count == 0)
            return;
        this.scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(IExpression expression, Token name)
    {
        var i = 0;
        foreach (var scope in this.scopes.Reverse())
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                this.interpreter.Resolve(expression, i);
                return;
            }
            i++;
        }
    }

    public void Resolve(IEnumerable<IStatement> statements)
    {
        foreach (var statement in statements)
            this.Resolve(statement);
    }
    public void Resolve(IStatement statement) => this.ResolveStatement((dynamic)statement);
    public void Resolve(IExpression expression) => this.ResolveExpression((dynamic)expression);

    private void ResolveExpression(IExpression expression) => throw new NotImplementedException();
    private void ResolveStatement(IStatement statement) => throw new NotImplementedException();
}
