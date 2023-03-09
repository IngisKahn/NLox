namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Assign(Token Name, IExpression Value) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Assignment()
        {
            var expression = await this.Ternary();

            if (!this.Match(TokenType.Equal))
                return expression;

            var equals = this.Previous;
            var value = await this.Assignment();

            if (expression is Variable v)
                return new Assign(v.Name, value);
            else if (expression is Get g)
                return new Set(g.Object, g.Name, value);

            await Error(equals, "Invalid assignment target.");
            return expression;
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Assign assign)
        {
            var value = this.Evaluate(assign.Value);
            if (this.locals.TryGetValue(assign, out var distance))
                this.Scope.AssignAt(distance, assign.Name, value);
            else
                this.Globals.Assign(assign.Name, value);
            return value;
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Assign assign)
        {
            this.Resolve(assign.Value);
            this.ResolveLocal(assign, assign.Name);
        }
    }
}
