namespace NLox.Interpreter.Expressions;

public record Call(IExpression Callee, Token Paren, IList<IExpression> Arguments) : IExpression;
