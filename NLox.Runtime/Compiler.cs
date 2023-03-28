#define DEBUG_TRACE_EXECUTION
#define DEBUG_PRINT_CODE
using System.Text;

namespace NLox.Runtime;

public unsafe class Compiler
{
    private readonly Parser parser;
    private readonly Chunk chunk;
    private readonly Action<IntPtr> registerObject;
    private readonly Table strings;

    public Compiler(string source, Chunk chunk, Action<IntPtr> registerObject, Table strings)
    {
        this.parser = new(source); 
        this.chunk = chunk;
        this.registerObject = registerObject;
        this.strings = strings;
    }

    public bool Compile()
    {
        parser.Advance();
        while (!this.parser.Match(TokenType.EoF))
            this.Declaration();
        this.EndCompiler();
        return !parser.HadError;
    }

    public void Declaration()
    {
        if (this.parser.Match(TokenType.Var))
            this.VarDeclaration();
        else
            this.Statement();
        if (this.parser.PanicMode)
            parser.Synchronize();
    }

    private void VarDeclaration()
    {
        var global = this.ParseVariable("Expect variable name.");
        if (this.parser.Match(TokenType.Equal))
            this.Expression();
        else
            this.EmitByte((byte)OpCode.Nil);

        this.parser.Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");

        this.DefineVariable(global);
    }

    private void DefineVariable(byte global) =>
        this.EmitByte((byte) OpCode.DefineGlobal);

    public byte ParseVariable(string errorMessage)
    {
        this.parser.Consume(TokenType.Identifier, errorMessage);
        return this.IdentifierConstant(this.parser.Previous);
    }

    private byte IdentifierConstant(Token name)
    {
        var chars = Encoding.ASCII.GetBytes(name.Lexeme[name.Start..(name.Start + name.Length)]);
        fixed (byte* pChars = chars)
        {
            return this.MakeConstant(
                ObjectString.CopyString(pChars, this.parser.Previous.Length - 2, registerObject, strings));
        }
    }

    public void Statement()
    {
        if (this.parser.Match(TokenType.Print))
            this.PrintStatement();
        else
            this.ExpressionStatement();
    }

    public void ExpressionStatement()
    {
        this.Expression();
        this.parser.Consume(TokenType.Semicolon, "Expect ';' after expression.");
        this.EmitByte((byte)OpCode.Pop);
    }

    public void PrintStatement()
    {
        this.Expression();
        this.parser.Consume(TokenType.Semicolon, "Expect ';' after value.");
        this.EmitByte((byte)OpCode.Print);
    }

    private void EmitByte(byte b) => chunk.Write(b, parser.Previous.Line);
    private void EmitBytes(byte b1, byte b2)
    {
        this.EmitByte(b1);
        this.EmitByte(b2);
    }

    private void EndCompiler()
    {
        this.EmitReturn();
#if DEBUG_PRINT_CODE
        if (!this.parser.HadError) 
            Common.DisassembleChunk(this.chunk, "code");
#endif
    }

    private void EmitReturn() => this.EmitByte((byte)OpCode.Return);

    private void Expression()
    {
        this.ParsePrecedence(Precedence.Comma);
    }

    private void Number(bool canAssign) => this.EmitConstant(double.Parse(parser.Previous.Lexeme));

    private void EmitConstant(Value value) => this.EmitBytes((byte)OpCode.Constant, this.MakeConstant(value));

    private byte MakeConstant(Value value) 
    {
        var constant = chunk.AddConstant(value);
        if (constant <= byte.MaxValue)
            return (byte)constant;
        parser.Error("Too many constants in one chunk.");
        return 0;
    }

    private void Grouping(bool canAssign)
    {
        this.Expression();
        parser.Consume(TokenType.RightParen, "Expect ')' after expression.");
    }
    private void Unary(bool canAssign)
    {
        var operatorType = parser.Previous.Type;

        this.ParsePrecedence(Precedence.Unary);

        switch (operatorType)
        {
            case TokenType.Bang:
                this.EmitByte((byte)OpCode.Not);
                break;
            case TokenType.Minus:
                this.EmitByte((byte)OpCode.Negate);
                break;
            default:
                return;
        }
    }
    private void Binary(bool canAssign)
    {
        var operatorType = parser.Previous.Type;
        this.ParsePrecedence(GetPrecedence(operatorType) + 1);

        switch (operatorType)
        {
            case TokenType.BangEqual:
                this.EmitBytes((byte)OpCode.Equal, (byte)OpCode.Not);
                break;
            case TokenType.EqualEqual:
                this.EmitByte((byte)OpCode.Equal);
                break;
            case TokenType.Greater:
                this.EmitByte((byte)OpCode.Greater);
                break;
            case TokenType.GreaterEqual:
                this.EmitBytes((byte)OpCode.Less, (byte)OpCode.Not);
                break;
            case TokenType.Less:
                this.EmitByte((byte)OpCode.Less);
                break;
            case TokenType.LessEqual:
                this.EmitBytes((byte)OpCode.Greater, (byte)OpCode.Not);
                break;
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

        var canAssign = precedence <= Precedence.Assignment;
        prefixRule(canAssign);

        while(precedence <= GetPrecedence(this.parser.Current.Type))
        {
            this.parser.Advance();
            Infix(this.parser.Previous.Type)?.Invoke(canAssign);
        }

        if (canAssign && this.parser.Match(TokenType.Equal))
            this.parser.Error("Invalid assignment type.");
    }


    private Action<bool>? GetPrefix(TokenType type) =>
        type switch
        {
            TokenType.Bang => this.Unary,
            TokenType.False => this.Literal,
            TokenType.LeftParen => this.Grouping,
            TokenType.Minus => this.Unary,
            TokenType.Nil => this.Literal,
            TokenType.Number => this.Number,
            TokenType.True => this.Literal,
            TokenType.String => this.String,
            TokenType.Identifier => this.Variable,
            _ => null
        };

    private Action<bool>? Infix(TokenType type) =>
        type switch 
        { 
            TokenType.EqualEqual or
                TokenType.Greater or
                TokenType.GreaterEqual or
                TokenType.Less or
                TokenType.LessEqual or
            TokenType.BangEqual 
                or TokenType.Minus 
                or TokenType.Plus 
                or TokenType.Star 
                or TokenType.Slash => this.Binary, 
            _ => null
        };

    private Precedence GetPrecedence(TokenType type) =>
        type switch
        {
            TokenType.Greater or TokenType.GreaterEqual or TokenType.Less or TokenType.LessEqual => Precedence.Comparison,
            TokenType.BangEqual or TokenType.EqualEqual => Precedence.Equality,
            TokenType.Minus or TokenType.Plus => Precedence.Term,
            TokenType.Star or TokenType.Slash => Precedence.Factor,
            _ => Precedence.None
        };

    private void Variable(bool canAssign) => this.NamedVariable(this.parser.Previous, canAssign);

    private void NamedVariable(Token name, bool canAssign)
    {
        var arg = this.IdentifierConstant(name);
        if (canAssign && this.parser.Match(TokenType.Equal))
        {
            this.Expression();
            this.EmitBytes((byte)OpCode.SetGlobal, arg);
        }
        else
            this.EmitBytes((byte)OpCode.GetGlobal, arg);
    }

    private void Literal(bool canAssign)
    {
        switch (this.parser.Previous.Type)
        {
            case TokenType.False:
                this.EmitByte((byte)OpCode.False);
                break;
            case TokenType.Nil:
                this.EmitByte((byte)OpCode.Nil);
                break;
            case TokenType.True:
                this.EmitByte((byte)OpCode.True);
                break;
        }
    }

    private unsafe void String(bool canAssign)
    {
        var p = this.parser.Previous;
        var chars = Encoding.ASCII.GetBytes(p.Lexeme[(p.Start + 1)..(p.Start + p.Length - 2)]);
        fixed (byte* pChars = chars)
        {
            this.EmitConstant(
                ObjectString.CopyString(pChars, this.parser.Previous.Length - 2, registerObject, strings));
        }
    }
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
    public bool PanicMode { get; private set; }

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


    public bool Match(TokenType type)
    {
        if (!this.Check(type))
            return false;
        this.Advance();
        return true;
    }

    public bool Check(TokenType type) => this.Current.Type == type;

    private void ErrorAtCurrent(string message) => this.ErrorAt(this.Current, message);

    public void Error(string message) => this.ErrorAt(this.Previous, message);

    private void ErrorAt(Token token, string message)
    {
        if (this.PanicMode)
            return;
        this.PanicMode = true;
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

    public void Synchronize()
    {
        this.PanicMode = false;

        while (this.Current.Type != TokenType.EoF)
        {
            if (this.Previous.Type == TokenType.Semicolon)
                return;
            switch (this.Current.Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }
            this.Advance();
        }
    }
}
