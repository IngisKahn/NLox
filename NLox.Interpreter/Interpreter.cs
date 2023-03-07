namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;

public class Interpreter
{
    private readonly Scope globals = new();
    private Scope scope;

    public Interpreter()
    {
        this.scope = this.globals;
        this.globals.Define("clock", new Clock());
    }

    private class Clock : ICallable
    {
        public int Arity => 0;

        public object? Call(Interpreter interpreter, IList<object> arguments) => DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;

        public override string ToString() => "<native fn>";
    }

    private enum BreakMode
    {
        None,
        Break,
        Continue
    }

    private BreakMode breakMode;

    public void Interpret(IStatement statement)
    {
        if (this.breakMode == BreakMode.None || statement is LoopStatement)
            this.EvaluateStatement(statement);
    }

    private void EvaluateStatement(VarStatement statement) => this.scope.Define(statement.Name.Lexeme, statement.Expression != null ? this.Evaluate(statement.Expression) : null);


    private void EvaluateStatement(IStatement statement) => this.EvaluateStatement((dynamic)statement);

    private void EvaluateStatement(If ifStatement)
    {
        if (IsTruthy(this.Evaluate(ifStatement.Condition)))
            this.EvaluateStatement(ifStatement.Then);
        else if (ifStatement.Else != null)
            this.EvaluateStatement(ifStatement.Else);
    }

    private void EvaluateStatement(Block block)
    {
        var previous = this.scope;
        this.scope = new Scope(previous);
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
            this.scope = previous;
        }
    }

    private void EvaluateStatement(ExpressionStatement expressionStatement) => this.Evaluate(expressionStatement.Expression);

    private void EvaluateStatement(PrintStatement printStatement) =>
        Console.WriteLine(Stringify(this.Evaluate(printStatement.Expression)));

    public object? Evaluate(IExpression expression) => this.Evaluate((dynamic)expression);

    private object? Evaluate(Assign assign)
    {
        var value = this.Evaluate(assign.Value);
        this.scope.Assign(assign.Name, value);
        return value;
    }

    private object? Evaluate(Call call)
    {
        if (this.Evaluate(call.Callee) is not ICallable callee)
            throw new RuntimeException(call.Paren, "Can only call functions and classes.");

        if (call.Arguments.Count != callee.Arity)
            throw new RuntimeException(call.Paren, $"Expected {callee.Arity} arguments, but got {call.Arguments.Count}.");

        var arguments = call.Arguments.Select(this.Evaluate).Where(a => a != null).Select(a => a!).ToArray();

        return callee.Call(this, arguments);
    }

    private interface ICallable
    {
        int Arity { get; }
        object? Call(Interpreter interpreter, IList<object> arguments);
    }

    private object? Evaluate(Variable variable) => this.scope[variable.Name];

    private object? Evaluate(Literal literal) => literal.Value;
    private object? Evaluate(Grouping grouping) => this.Evaluate(grouping.Expression);
    private object? Evaluate(Unary unary)
    {
        var right = this.Evaluate(unary.Right);

        return unary.Operator.Type == TokenType.Minus && right is not double
            ? throw new RuntimeException(unary.Operator, "Unary negation applied to non-number")
            : unary.Operator.Type switch
            {
                TokenType.Minus => -(double)(right ?? 0),
                TokenType.Bang => !IsTruthy(right),
                _ => null
            };
    }

    private object? Evaluate(Logical logical)
    {
        var left = this.Evaluate(logical.Left);
        if (logical.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
        }
        else if (!IsTruthy(left))
            return left;

        return this.Evaluate(logical.Right);
    }

    private void EvaluateStatement(LoopStatement loopStatement)
    {
        if (loopStatement.Initializer != null)
            this.EvaluateStatement(loopStatement.Initializer);

        while (loopStatement.Condition == null || IsTruthy(this.Evaluate(loopStatement.Condition)))
        {
            this.EvaluateStatement(loopStatement.Body);
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

    private void EvaluateStatement(ControlFlow controlFlow) => this.breakMode = controlFlow.Token.Type == TokenType.Continue ? BreakMode.Continue : BreakMode.Break;

    private object? Evaluate(Binary binary)
    {
        var left = this.Evaluate(binary.Left);
        var right = this.Evaluate(binary.Right);

        switch (binary.Operator.Type)
        {
            case TokenType.Greater:
            case TokenType.GreaterEqual:
            case TokenType.Less:
            case TokenType.LessEqual:
            case TokenType.Minus:
            case TokenType.Slash:
            case TokenType.Star:
                if (left is not double || right is not double)
                    throw new RuntimeException(binary.Operator, "Operator requires numbers");
                break;
        }

        return binary.Operator.Type switch
        {
            TokenType.Greater => (double)(left ?? 0) > (double)(right ?? 0),
            TokenType.GreaterEqual => (double)(left ?? 0) >= (double)(right ?? 0),
            TokenType.Less => (double)(left ?? 0) < (double)(right ?? 0),
            TokenType.LessEqual => (double)(left ?? 0) <= (double)(right ?? 0),
            TokenType.BangEqual => !IsEqual(left, right),
            TokenType.EqualEqual => IsEqual(left, right),
            TokenType.Minus => (double)(left ?? 0) - (double)(right ?? 0),
            TokenType.Slash => (double)(left ?? 0) / (double)(right ?? 0),
            TokenType.Star => (double)(left ?? 0) * (double)(right ?? 0),
            TokenType.Plus when left is string s1 => s1 + right?.ToString(),
            TokenType.Plus when right is string s1 => left?.ToString() + s1,
            TokenType.Plus when left is double d1 && right is double d2 => d1 + d2,
            TokenType.Plus => throw new RuntimeException(binary.Operator, "Plus operator can only be used on two numbers or two strings"),
            TokenType.Comma => right,
            _ => null
        }; ;
    }

    private object? Evaluate(Ternary ternary) =>
        IsTruthy(this.Evaluate(ternary.Test)) ? this.Evaluate(ternary.Left) : this.Evaluate(ternary.Right);

    private static bool IsTruthy(object? value) => value != null && (value is not bool b || b);

    private static bool IsEqual(object? a, object? b) =>
        a == null ? b == null : a.Equals(b);

    public static string Stringify(object? obj)
    {
        if (obj is null)
            return "nil";

        if (obj is double d)
        {
            var text = d.ToString();
            if (text.EndsWith(".0"))
                text = text[..^3];
            return text;
        }

        return obj.ToString()!;
    }
}
