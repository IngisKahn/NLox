namespace NLox.Interpreter
{
    using Expressions;
    using Statements;

    namespace Statements
    {
        public record ExpressionStatement(IExpression Expression) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> ExpressionStatement()
        {
            var value = await this.Expression();
            await this.Consume(TokenType.Semicolon, "Expect ';' after expression");
            return new ExpressionStatement(value);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(ExpressionStatement expressionStatement) => this.Evaluate(expressionStatement.Expression);
    }

    //public partial class Resolver 
    //{
    //}
}
