namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Variable(Token Name) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Variable variable) => this.Scope[variable.Name];
    }

    //public partial class Resolver 
    //{
    //}
}
