namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Set(IExpression Object, Token Name, object? Value) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Set set)
        {
            var @object = this.Evaluate(set.Object);
            var value = set.Value is IExpression e ? this.Evaluate(e) : set.Value;
            (@object is Instance i ? i : throw new RuntimeException(set.Name, "Only instances have fields."))[set.Name] = value;
            return value;
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Set set)
        {
            if (set.Value is IExpression e)
                this.Resolve(e);
            this.Resolve(set.Object);
        }
    }
}