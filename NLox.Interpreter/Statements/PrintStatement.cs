namespace NLox.Interpreter
{
    using Expressions;
    using Statements;

    namespace Statements
    {
        public record PrintStatement(IExpression Expression) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> PrintStatement()
        {
            var value = await this.Expression();
            if (value == null)
                throw new ParsingException("Expect expression after 'print'");
            await this.Consume(TokenType.Semicolon, "Expect ';' after expression");
            return new PrintStatement(value);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(PrintStatement printStatement) =>
            Console.WriteLine(Stringify(this.Evaluate(printStatement.Expression)));
    }

    public partial class Resolver 
    {
        private void ResolveStatement(PrintStatement printStatement) =>
            this.Resolve(printStatement.Expression);
    }
}
