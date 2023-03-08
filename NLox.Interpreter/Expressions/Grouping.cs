namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Grouping(IExpression Expression) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Grouping grouping) => this.Evaluate(grouping.Expression);
    }

    public partial class Resolver
    {
        private void ResolveExpression(Grouping grouping) => this.Resolve(grouping.Expression);
    }
}

