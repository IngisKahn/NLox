namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Literal(object? Value) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Literal literal) => literal.Value;
    }

    //public partial class Resolver 
    //{
    //}
}
