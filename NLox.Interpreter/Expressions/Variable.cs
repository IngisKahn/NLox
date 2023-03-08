namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Variable(Token Name) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Variable variable) => this.LookUpVariable(variable.Name, variable);
    }

    public partial class Resolver 
    {
        public void ResolveExpression(Variable variable) 
        {
            if (this.scopes.Count != 0 && this.scopes.Peek().TryGetValue(variable.Name.Lexeme, out var init) && !init)
                throw new RuntimeException(variable.Name, "Can't read local variable in its own initializer.");
            this.ResolveLocal(variable, variable.Name);
        }
    }
}
