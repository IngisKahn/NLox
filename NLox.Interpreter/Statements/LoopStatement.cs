namespace NLox.Interpreter
{
    using Expressions;
    using Statements;

    namespace Statements
    {
        public record LoopStatement(IStatement? Initializer, IExpression? Condition, IExpression? Increment, IStatement Body) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> For()
        {
            await this.Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

            IStatement? initializer;
            if (this.Match(TokenType.Semicolon))
                initializer = null;
            else if (this.Match(TokenType.Var))
                initializer = await VariableDeclaration();
            else
                initializer = await ExpressionStatement();
            IExpression? condition = null;
            if (!this.Check(TokenType.Semicolon))
                condition = await Expression();
            await this.Consume(TokenType.Semicolon, "Expect ';' after for condition.");
            IExpression? increment = null;
            if (!this.Check(TokenType.RightParen))
                increment = await Expression();
            await this.Consume(TokenType.RightParen, "Expect ')' after for clauses.");
            var body = await this.Statement();

            return new LoopStatement(initializer, condition, increment, body);
        }

        private async Task<IStatement> WhileStatement()
        {
            await this.Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
            var condition = await this.Expression();
            if (condition == null)
                throw new ParsingException("Expect expression after 'while ('");
            await this.Consume(TokenType.RightParen, "Expect ')' after 'while'.");

            return new LoopStatement(null, condition, null, await this.Statement());
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(LoopStatement loopStatement)
        {
            if (loopStatement.Initializer != null)
                this.Evaluate(loopStatement.Initializer);

            while (loopStatement.Condition == null || IsTruthy(this.Evaluate(loopStatement.Condition)))
            {
                this.Evaluate(loopStatement.Body);
                var bail = false;
                switch (this.breakMode)
                {
                    case BreakMode.Continue:
                        if (loopStatement.Condition != null)
                            this.breakMode = BreakMode.None;
                        else
                            bail = true;
                        break;
                    case BreakMode.Break:
                        bail = true;
                        this.breakMode = BreakMode.None;
                        break;
                }
                if (bail)
                    break;
                if (loopStatement.Increment != null)
                    this.Evaluate(loopStatement.Increment);
            }
        }
    }

    //public partial class Resolver 
    //{
    //}
}
