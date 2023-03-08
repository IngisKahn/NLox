namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Unary(Token Operator, IExpression Right) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Unary()
        {
            if (this.Match(TokenType.Bang, TokenType.Minus))
            {
                var @operator = this.Previous;
                var right = await this.Unary();
                return new Unary(@operator, right);
            }

            return await this.Call();
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Unary unary)
        {
            var right = this.Evaluate(unary.Right);

            return unary.Operator.Type == TokenType.Minus && right is not double
                ? throw new RuntimeException(unary.Operator, "Unary negation applied to non-number")
                : unary.Operator.Type switch
                {
                    TokenType.Minus => -(double)(right ?? 0),
                    TokenType.Bang => !IsTruthy(right),
                    _ => null
                };
        }
    }

    public partial class Resolver 
    {
        private void ResolveExpression(Unary unary) => this.Resolve(unary.Right);
    }
}
