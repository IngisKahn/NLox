namespace NLox.Interpreter
{
    using Statements;

    namespace Statements
    {
        public record Block(IList<IStatement> Statements) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> Block()
        {
            List<IStatement> statements = new();

            while (!Check(TokenType.RightBrace) && !this.IsAtEnd)
            {
                var statement = await Declaration();
                if (statement != null)
                    statements.Add(statement);
            }

            await this.Consume(TokenType.RightBrace, "Expect '}' after block.");

            return new Block(statements);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(Block block) =>
            this.ExecuteBlock(block, new(this.Scope));

        public void ExecuteBlock(Block block, Scope scope)
        {
            var previous = this.Scope;
            this.Scope = scope;
            try
            {
                foreach (var statement in block.Statements)
                {
                    this.Interpret(statement);
                    if (breakMode != BreakMode.None)
                        break;
                }
            }
            finally
            {
                this.Scope = previous;
            }
        }
    }

    //public partial class Resolver 
    //{
    //}
}
