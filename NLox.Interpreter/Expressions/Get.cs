namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Get(IExpression Object, Token Name) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Get get)
        {
            var @object = this.Evaluate(get.Object);
            return @object is Instance i ? i[get.Name] : throw new RuntimeException(get.Name, "Only instances have properties.");
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Get get) => this.Resolve(get.Object);
    }
}
