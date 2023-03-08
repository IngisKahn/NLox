namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Logical(IExpression Left, Token Operator, IExpression Right) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Or()
        {
            var expression = await this.And();
            while (this.Match(TokenType.Or))
            {
                var @operator = this.Previous;
                expression = new Logical(expression, @operator, await this.And());
            }

            return expression;
        }
        private async Task<IExpression> And()
        {
            var expression = await this.Equality();
            while (this.Match(TokenType.And))
            {
                var @operator = this.Previous;
                expression = new Logical(expression, @operator, await this.Equality());
            }

            return expression;
        }

    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Logical logical)
        {
            var left = this.Evaluate(logical.Left);
            if (logical.Operator.Type == TokenType.Or)
            {
                if (IsTruthy(left))
                    return left;
            }
            else if (!IsTruthy(left))
                return left;

            return this.Evaluate(logical.Right);
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Logical logical)
        {
            this.Resolve(logical.Left);
            this.Resolve(logical.Right);
        }
    }
}