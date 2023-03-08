namespace NLox.Interpreter
{
    using Expressions;
    using Statements;

    namespace Statements
    {
        public record If(IExpression Condition, IStatement Then, IStatement? Else = null) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> If()
        {
            await this.Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
            var condition = await this.Expression();
            await this.Consume(TokenType.RightParen, "Expect ')' after if condition.");

            var thenBranch = await this.Statement();
            return this.Match(TokenType.Else) ? new If(condition, thenBranch, await this.Statement()) :
                new If(condition, thenBranch);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(If ifStatement)
        {
            if (IsTruthy(this.Evaluate(ifStatement.Condition)))
                this.Evaluate(ifStatement.Then);
            else if (ifStatement.Else != null)
                this.Evaluate(ifStatement.Else);
        }
    }

    //public partial class Resolver 
    //{
    //}
}