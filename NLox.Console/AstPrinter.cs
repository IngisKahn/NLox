using NLox.Interpreter.Expressions;
using System.Text;

public static class AstPrinter
{
    public static string Print(IExpression expression) => AstPrinter.Print((dynamic)expression);

    private static string Print(Binary expression) => AstPrinter.Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
    private static string Print(Grouping expression) => AstPrinter.Parenthesize("group", expression.Expression);
    private static string Print(Literal expression) => expression.Value?.ToString() ?? "nil";
    private static string Print(Unary expression) => AstPrinter.Parenthesize(expression.Operator.Lexeme, expression.Right);

    private static string Parenthesize(string name, params IExpression[] expressions)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);
        foreach (var expression in expressions)
        {
            builder.Append(' ');
            builder.Append(AstPrinter.Print(expression));
        }
        builder.Append(')');

        return builder.ToString();
    }
}