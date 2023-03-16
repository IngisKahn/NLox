#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public class Compiler
{
    private readonly Parser parser;
    private readonly Chunk chunk;

    public Compiler(string source, Chunk chunk)
    {
        this.parser = new(source); 
        this.chunk = chunk;
    }

    public bool Compile()
    {
        parser.Advance();
        this.Expression();
        parser.Consume(TokenType.EoF, "Expect end of stream.");
        this.EndCompiler();
        return !parser.HadError;
    }

    private void EmitByte(byte b) => chunk.Write(b, parser.Previous.Line);
    private void EmitBytes(byte b1, byte b2)
    {
        this.EmitByte(b1);
        this.EmitByte(b2);
    }

    private void EndCompiler() => this.EmitReturn();

    private void EmitReturn() => this.EmitByte((byte)OpCode.Return);

    private void Expression()
    {
        this.ParsePrecedence(Precedence.Comma);
    }

    private void Number() => this.EmitConstant(double.Parse(parser.Previous.Lexeme));

    private void EmitConstant(Value value) => this.EmitBytes((byte)OpCode.Constant, this.MakeConstant(value));

    private byte MakeConstant(Value value) 
    {
        var constant = chunk.AddConstant(value);
        if (constant <= byte.MaxValue)
            return (byte)constant;
        parser.Error("Too many constants in one chunk.");
        return 0;
    }

    private void Grouping()
    {
        this.Expression();
        parser.Consume(TokenType.RightParen, "Expect ')' after expression.");
    }
    private void Unary()
    {
        var operatorType = parser.Previous.Type;

        this.ParsePrecedence(Precedence.Unary);

        switch (operatorType)
        {
            case TokenType.Minus:
                this.EmitByte((byte)OpCode.Negate);
                break;
            default:
                return;
        }
    }
    private void Binary()
    {
        var operatorType = parser.Previous.Type;
        this.ParsePrecedence((Precedence)(GetPrecedence(operatorType) + 1));

        switch (operatorType)
        {
            case TokenType.Plus:
                this.EmitByte((byte)OpCode.Add);
                break;
            case TokenType.Minus:
                this.EmitByte((byte)OpCode.Subtract);
                break;
            case TokenType.Star:
                this.EmitByte((byte)OpCode.Multiply);
                break;
            case TokenType.Slash:
                this.EmitByte((byte)OpCode.Divide);
                break;
            default:
                return;
        }
    }

    private void ParsePrecedence(Precedence precedence)
    {
        this.parser.Advance();
        var prefixRule = this.GetPrefix(this.parser.Previous.Type);
        if (prefixRule == null)
        {
            this.parser.Error("Expect expression.");
            return;
        }
        
        prefixRule();

        while(precedence <= GetPrecedence(this.parser.Current.Type))
        {
            this.parser.Advance();
            Infix(this.parser.Previous.Type)?.Invoke();
        }
    }


    private Action? GetPrefix(TokenType type) =>
        type switch
        {
            TokenType.LeftParen => this.Grouping,
            TokenType.Minus => this.Unary,
            TokenType.Number => this.Number,
            _ => null
        };

    private Action? Infix(TokenType type) =>
        type switch 
        { 
            TokenType.Minus or TokenType.Plus or TokenType.Star or TokenType.Slash => this.Binary, 
            _ => null
        };

    private Precedence GetPrecedence(TokenType type) =>
        type switch
        {
            TokenType.Minus or TokenType.Plus => Precedence.Term,
            TokenType.Star or TokenType.Slash => Precedence.Factor,
            _ => Precedence.None
        };

}
    internal enum Precedence
    {
        None,
        Comma,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term, 
        Factor,
        Unary,
        Call,
        Primary
    }

public class Parser
{
    private readonly Scanner scanner;
    public Token Previous { get; private set; } = new Token(TokenType.Error, string.Empty, 0, 0, 0);
    public Token Current { get; private set; } = new Token(TokenType.Error, string.Empty, 0, 0, 0);
    public bool HadError { get; private set; }
    private bool panicMode;

    public Parser(string source) => this.scanner = new(source);

    public void Advance()
    {
        this.Previous = this.Current;

        for (; ; )
        {
            this.Current = this.scanner.ScanToken();
            if (this.Current.Type == TokenType.Error)
                break;

            this.ErrorAtCurrent(this.Current.Source[..this.Current.Start]);
        }
    }

    private void ErrorAtCurrent(string message) => this.ErrorAt(this.Current, message);

    public void Error(string message) => this.ErrorAt(this.Previous, message);

    private void ErrorAt(Token token, string message)
    {
        if (this.panicMode)
            return;
        this.panicMode = true;
        var err = Console.Error;
        err.Write($"[line {token.Line}] Error");

        switch (token.Type)
        {
            case TokenType.EoF:
                err.Write(" at end");
                break;
                case TokenType.Error:
                break;
                default:
                err.Write($" at '{token.Source[token.Start..(token.Start + token.Length)]}'");
                break;
        }

        err.WriteLine(": " + message);
        this.HadError = true;
    }

    public void Consume(TokenType type, string message) 
    {
        if (this.Current.Type == type)
            this.Advance();
        else
            this.ErrorAtCurrent(message);
    }
}
