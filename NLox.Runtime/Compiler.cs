#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public class Compiler
{
    public void Compile(string source)
    {
        Scanner scanner = new(source);
        var line = -1;
        Token token;
        do
        {
            token = scanner.ScanToken();
            if (token.Line != line)
            {
                Console.Write($"{token.Line,4} ");
                line = token.Line;
            }
            else
                Console.Write("   | ");
            Console.WriteLine($"{token.Type,2} '{source[token.Start..(token.Start + token.Length)]}'");

        } while (token.Type != TokenType.EoF);
    }
}
