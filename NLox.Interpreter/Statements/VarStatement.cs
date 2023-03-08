namespace NLox.Interpreter
{
    using Expressions;
    using Statements;

    namespace Statements
    {
        public record VarStatement(Token Name, IExpression? Expression) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> VariableDeclaration()
        {
            var name = await this.Consume(TokenType.Identifier, "Expect variable name.");

            var initializer = this.Match(TokenType.Equal) ? await this.Expression() : null;

            await this.Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new VarStatement(name, initializer);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(VarStatement statement) => this.Scope.Define(statement.Name.Lexeme, statement.Expression != null ? this.Evaluate(statement.Expression) : null);
    }

    public partial class Resolver 
    {
        private void ResolveStatement(VarStatement varStatement)
        {
            this.Declare(varStatement.Name);
            if (varStatement.Expression != null)
                this.Resolve(varStatement.Expression);
            this.Define(varStatement.Name);
        }
    }
}
