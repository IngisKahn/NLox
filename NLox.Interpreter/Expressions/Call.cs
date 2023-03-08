namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Call(IExpression Callee, Token Paren, IList<IExpression> Arguments) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Call()
        {
            var expression = await this.Primary();

            while (this.Match(TokenType.LeftParen))
                expression = await this.FinishCall(expression);

            return expression;
        }
        private async Task<IExpression> FinishCall(IExpression callee)
        {
            List<IExpression> arguments = new();
            if (!this.Check(TokenType.RightParen))
                do
                {
                    if (arguments.Count >= 255)
                        await this.Error(this.Peek, "Can't have more than 255 arguments.");
                    arguments.Add(await this.Assignment());
                }
                while (this.Match(TokenType.Comma));

            var paren = await this.Consume(TokenType.RightParen, "Expect ')' after arguments.");
            return new Call(callee, paren, arguments);
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Call call)
        {
            if (this.Evaluate(call.Callee) is not ICallable callee)
                throw new RuntimeException(call.Paren, "Can only call functions and classes.");

            if (call.Arguments.Count != callee.Arity)
                throw new RuntimeException(call.Paren, $"Expected {callee.Arity} arguments, but got {call.Arguments.Count}.");

            var arguments = call.Arguments.Select(this.Evaluate).Where(a => a != null).Select(a => a!).ToArray();

            var result = callee.Call(this, arguments);
            if (breakMode == BreakMode.Return)
                breakMode = BreakMode.None;
            return result;
        }
    }
}
