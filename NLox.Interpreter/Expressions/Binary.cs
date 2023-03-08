namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Binary(IExpression Left, Token Operator, IExpression Right) : IExpression;
    }

    public partial class Parser
    {
        private async Task<IExpression> Equality()
        {
            var expression = await this.Comparison();

            while (this.Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                var @operator = this.Previous;
                var right = await this.Comparison();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }
        private async Task<IExpression> Comparison()
        {
            var expression = await this.Term();

            while (this.Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var @operator = this.Previous;
                var right = await this.Term();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private async Task<IExpression> Term()
        {
            var expression = await this.Factor();

            while (this.Match(TokenType.Minus, TokenType.Plus))
            {
                var @operator = this.Previous;
                var right = await this.Factor();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }
        private async Task<IExpression> Factor()
        {
            var expression = await this.Unary();

            while (this.Match(TokenType.Slash, TokenType.Star))
            {
                var @operator = this.Previous;
                var right = await this.Unary();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private async Task<IExpression> Comma()
        {
            var expression = await this.Assignment();

            while (this.Match(TokenType.Comma))
            {
                var @operator = this.Previous;
                var right = await this.Assignment();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Binary binary)
        {
            var left = this.Evaluate(binary.Left);
            var right = this.Evaluate(binary.Right);

            switch (binary.Operator.Type)
            {
                case TokenType.Greater:
                case TokenType.GreaterEqual:
                case TokenType.Less:
                case TokenType.LessEqual:
                case TokenType.Minus:
                case TokenType.Slash:
                case TokenType.Star:
                    if (left is not double || right is not double)
                        throw new RuntimeException(binary.Operator, "Operator requires numbers");
                    break;
            }

            return binary.Operator.Type switch
            {
                TokenType.Greater => (double)(left ?? 0) > (double)(right ?? 0),
                TokenType.GreaterEqual => (double)(left ?? 0) >= (double)(right ?? 0),
                TokenType.Less => (double)(left ?? 0) < (double)(right ?? 0),
                TokenType.LessEqual => (double)(left ?? 0) <= (double)(right ?? 0),
                TokenType.BangEqual => !IsEqual(left, right),
                TokenType.EqualEqual => IsEqual(left, right),
                TokenType.Minus => (double)(left ?? 0) - (double)(right ?? 0),
                TokenType.Slash => (double)(left ?? 0) / (double)(right ?? 0),
                TokenType.Star => (double)(left ?? 0) * (double)(right ?? 0),
                TokenType.Plus when left is string s1 => s1 + right?.ToString(),
                TokenType.Plus when right is string s1 => left?.ToString() + s1,
                TokenType.Plus when left is double d1 && right is double d2 => d1 + d2,
                TokenType.Plus => throw new RuntimeException(binary.Operator, "Plus operator can only be used on two numbers or two strings"),
                TokenType.Comma => right,
                _ => null
            }; ;
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Binary binary)
        {
            this.Resolve(binary.Left);
            this.Resolve(binary.Right);
        }
    }
}