namespace NLox.Interpreter
{
    using global::NLox.Interpreter.Expressions;
    using Statements;

    namespace Statements
    {
        public record Return(Token keyword, IExpression? Value) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> ReturnStatement()
        {
            var keyword = this.Previous;
            var value = this.Check(TokenType.Semicolon) ? null : await this.Expression();

            await this.Consume(TokenType.Semicolon, "Expect ';' after return value");
            return new Return(keyword, value);
        }
    }

    public partial class Interpreter
    {
        private object? EvaluateStatement(Return returnStatement)
        {
            var value = returnStatement.Value != null ? this.Evaluate(returnStatement.Value) : null;
            this.breakMode = BreakMode.Return;
            this.ReturnValue = value;
            return value;
        }
    }

    public partial class Resolver
    {
        private void ResolveStatement(Return returnStatement)
        {
            if (currentFunction == FunctionType.None)
                throw new RuntimeException(returnStatement.keyword, "Can't return from top-level code.");
            if (returnStatement.Value == null)
                return;
            if (currentFunction == FunctionType.Initializer)
                throw new RuntimeException(returnStatement.keyword, "Can't return a value from an initializer.");
            this.Resolve(returnStatement.Value);
        }
    }
}