namespace NLox.Interpreter
{
    using Expressions;

    namespace Expressions
    {
        public record Super(Token Keyword, Token Method) : IExpression;
    }

    public partial class Interpreter
    {
        private object? EvaluateExpression(Super super)
        {
            var distance = this.locals[super];
            var superclass = (ClassCallable)this.Scope.GetAt(distance, "super")!;
            var @object = (Instance)this.Scope.GetAt(distance - 1, "this")!;
            var method = superclass.FindMethod(super.Method.Lexeme);\
            return method == null
                ? throw new RuntimeException(super.Method, $"Undefined property '{super.Method.Lexeme}'.")
                : (object)method.Bind(@object);
        }
    }

    public partial class Resolver
    {
        private void ResolveExpression(Super super)
        {
            switch (this.currentClass)
            {
                case ClassType.None:
                    throw new RuntimeException(super.Keyword, "Can't use 'super' outside of a class.");
                case ClassType.Subclass:
                    break;
                default:
                    throw new RuntimeException(super.Keyword, "Can't use 'super' in a class with no superclass.");
            }
            this.ResolveLocal(super, super.Keyword);
        }
    }
}