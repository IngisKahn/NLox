namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Ternary(IExpression Test, IExpression Left, IExpression Right) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Ternary()
        {
            Stack<IExpression> expressions = new();
            expressions.Push(await this.Or());

            while (this.Match(TokenType.Question))
            {
                expressions.Push(await this.Expression());
                await this.Consume(TokenType.Colon, "Missing false condition of ternary expression");
                expressions.Push(await this.Or());
            }

            while (expressions.Count > 1)
            {
                var right = expressions.Pop();
                var left = expressions.Pop();
                expressions.Push(new Ternary(expressions.Pop(), left, right));
            }

            return expressions.Pop();
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Ternary ternary) =>
            IsTruthy(this.Evaluate(ternary.Test)) ? this.Evaluate(ternary.Left) : this.Evaluate(ternary.Right);
    }

    //public partial class Resolver 
    //{
    //}
}
