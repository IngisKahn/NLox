namespace NLox.Interpreter
{
    using Statements;
    using System;

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
            this.Scope.Define(function.Name.Lexeme, new CallableFunction(function, this.Scope, false));
    }

    public partial class Resolver 
    {
        public void ResolveStatement(Function function)
        {
            this.Declare(function.Name);
            this.Define(function.Name);
            this.ResolveFunction(function, FunctionType.Function);
        }

        private void ResolveFunction(Function function, FunctionType functionType)
        {
            var enclosingFunction = this.currentFunction;
            currentFunction = functionType;
            this.BeginScope();
            foreach (var parameter in function.Parameters)
            {
                this.Declare(parameter);
                this.Define(parameter);
            }
            this.Resolve(function.Body);
            this.EndScope();
            currentFunction = enclosingFunction;
        }
    }
}
