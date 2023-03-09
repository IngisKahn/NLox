﻿namespace NLox.Interpreter
{
    using Statements;

    namespace Statements
    {
        public record Class(Token Name, IList<Function> Methods) : IStatement;
    }

    public partial class Parser
    {
        private async Task<IStatement> Class()
        {
            var name = await this.Consume(TokenType.Identifier, "Expect class name.");

            await this.Consume(TokenType.LeftBrace, "Expect '{' before class body.");
            List<Function> methods = new();

            while (!Check(TokenType.RightBrace) && !this.IsAtEnd)
                methods.Add((Function)await Function("method"));

            await this.Consume(TokenType.RightBrace, "Expect '}' after class body.");

            return new Class(name, methods);
        }
    }

    public partial class Interpreter
    {
        private void EvaluateStatement(Class @class)
        {
            this.Scope.Define(@class.Name.Lexeme, null);
            
            ClassCallable c = new(@class.Name.Lexeme, @class.Methods.ToDictionary(m => m.Name.Lexeme, m => new CallableFunction(m, this.Scope, m.Name.Lexeme == "init")));
            this.Scope.Assign(@class.Name, c);
        }
    }

    public partial class Resolver
    {
        private void ResolveStatement(Class @class)
        {
            var enclosingClass = this.currentClass;
            this.currentClass = ClassType.Class;
            this.Declare(@class.Name);
            this.Define(@class.Name);

            this.BeginScope();
            this.scopes.Peek()["this"] = true;

            foreach (var method in @class.Methods)
                this.ResolveFunction(method, method.Name.Lexeme == "init" ? FunctionType.Initializer : FunctionType.Method);

            this.EndScope();

            this.currentClass = enclosingClass;
        }
    }
}
