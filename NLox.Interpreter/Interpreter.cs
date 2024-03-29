﻿namespace NLox.Interpreter;

using NLox.Interpreter.Expressions;
using NLox.Interpreter.Statements;

public partial class Interpreter
{
    public Scope Globals { get; } = new();
    public Scope Scope { get; private set; }
    public object? ReturnValue { get; set; }
    private Dictionary<IExpression, int> locals = new();

    public Interpreter()
    {
        this.Scope = this.Globals;
        this.Globals.Define("clock", new Clock());
    }

    private enum BreakMode
    {
        None,
        Break,
        Continue,
        Return
    }

    private BreakMode breakMode;
    public void Interpret(IEnumerable<IStatement> statements)
    {
        foreach (var statement in statements)
            this.Interpret(statement);
    }
    public void Interpret(IStatement statement)
    {
        if (this.breakMode == BreakMode.None || statement is LoopStatement && this.breakMode != BreakMode.Return)
            this.Evaluate(statement);
    }

    private void Evaluate(IStatement statement) => this.EvaluateStatement((dynamic)statement);

    private object? EvaluateExpression(IExpression _) => throw new NotImplementedException();

    public object? Evaluate(IExpression expression) => this.EvaluateExpression((dynamic)expression);

    public void Resolve(IExpression expression, int depth) => this.locals[expression] = depth;

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
    private object? LookUpVariable(Token name, IExpression expression) => 
        this.locals.TryGetValue(expression, out var distance) 
        ? this.Scope.GetAt(distance, name.Lexeme) 
        : this.Globals[name];
}
