namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record This(Token Keyword) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(This @this) =>
            this.LookUpVariable(@this.Keyword, @this);
    }

    public partial class Resolver
    {
        private void ResolveExpression(This @this)
        {
            if (this.currentClass == ClassType.None)
                throw new RuntimeException(@this.Keyword, "Can't use 'this' outside of a class.");

            this.ResolveLocal(@this, @this.Keyword);
        }
    }
}
