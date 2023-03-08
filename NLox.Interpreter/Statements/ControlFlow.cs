namespace NLox.Interpreter
{
    using Statements;

    namespace Statements
    {
        public record ControlFlow(Token Token) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> ControlFlowStatement()
        {
            var token = this.Previous;
            await this.Consume(TokenType.Semicolon, $"Expect ';' after {token.Lexeme}");
            return new ControlFlow(token);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(ControlFlow controlFlow) => this.breakMode = controlFlow.Token.Type == TokenType.Continue ? BreakMode.Continue : BreakMode.Break;
    }

    //public partial class Resolver 
    //{
    //}
}