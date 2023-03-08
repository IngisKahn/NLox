namespace NLox.Interpreter
{
    using Statements;

    namespace Statements
    {
        public record Function(Token Name, IList<Token> Parameters, Block Body) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> Function(string kind)
        {
            var name = await this.Consume(TokenType.Identifier, $"Expect {kind} name.");
            await this.Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");
            List<Token> parameters = new();
            if (!this.Check(TokenType.RightParen))
                do
                {
                    if (parameters.Count >= 255)
                        await this.Error(this.Peek, "Can't have more than 255 parameters.");
                    parameters.Add(await this.Consume(TokenType.Identifier, "Expect parameter name."));
                } while (this.Match(TokenType.Comma));
            await this.Consume(TokenType.RightParen, "Expect ')' after parameters.");
            await this.Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
            return new Function(name, parameters, (Block)await this.Block());
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(Function function) =>
            this.Scope.Define(function.Name.Lexeme, new CallableFunction(function, this.Scope));
    }

    //public partial class Resolver 
    //{
    //}
}
