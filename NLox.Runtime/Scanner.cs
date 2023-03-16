#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

public class Scanner
{
    private readonly string source;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public Scanner(string source) => this.source = source;

    public Token ScanToken()
    {
        this.SkipWhitespace();
        this.start = this.current;

        if (this.IsAtEnd)
            return MakeToken(TokenType.EoF);

        var c = this.Advance();
        if (IsAlpha(c))
            return this.Identifier();
        if (IsDigit(c))
            return this.Number();

        return c switch
        {
            '(' => MakeToken(TokenType.LeftParen),
            ')' => MakeToken(TokenType.RightParen),
            '{' => MakeToken(TokenType.LeftBrace),
            '}' => MakeToken(TokenType.RightBrace),
            ';' => MakeToken(TokenType.Semicolon),
            ',' => MakeToken(TokenType.Comma),
            '.' => MakeToken(TokenType.Dot),
            '-' => MakeToken(TokenType.Minus),
            '+' => MakeToken(TokenType.Plus),
            '/' => MakeToken(TokenType.Slash),
            '*' => MakeToken(TokenType.Star),
            '?' => MakeToken(TokenType.Question),
            ':' => MakeToken(TokenType.Colon),
            '!' => MakeToken(this.Match('=') ? TokenType.BangEqual : TokenType.Bang),
            '=' => MakeToken(this.Match('=') ? TokenType.EqualEqual : TokenType.Equal),
            '<' => MakeToken(this.Match('=') ? TokenType.LessEqual : TokenType.Less),
            '>' => MakeToken(this.Match('=') ? TokenType.GreaterEqual : TokenType.Greater),
            '"' => this.String(),
            _ => ErrorToken("Unexpected character."),
        };
    }

    private TokenType IdentifierType()
    {
        switch (this.source[this.start])
        {
            case 'a': return CheckKeyword(1, 2, "nd", TokenType.And);
            case 'b': return CheckKeyword(1, 4, "reak", TokenType.Break);
            case 'c':
                if (this.current - this.start > 1)
                {
                    switch (this.source[this.start + 1])
                    {
                        case 'l':
                            return CheckKeyword(2, 3, "ass", TokenType.Class);
                        case 'o':
                            return CheckKeyword(2, 6, "ntinue", TokenType.Continue);
                    }
                }
                break;
            case 'e': return CheckKeyword(1, 3, "lse", TokenType.Else);
            case 'f':
                if (this.current - this.start > 1)
                {
                    switch (this.source[this.start + 1])
                    {
                        case 'a': return CheckKeyword(2, 3, "lse", TokenType.False);
                        case 'o': return CheckKeyword(2, 1, "r", TokenType.For);
                        case 'u': return CheckKeyword(2, 1, "n", TokenType.Fun);
                    }
                }
                break;
            case 'i': return CheckKeyword(1, 1, "f", TokenType.If);
            case 'n': return CheckKeyword(1, 2, "il", TokenType.Nil);
            case 'o': return CheckKeyword(1, 1, "r", TokenType.Or);
            case 'p': return CheckKeyword(1, 4, "rint", TokenType.Print);
            case 'r': return CheckKeyword(1, 5, "eturn", TokenType.Return);
            case 's': return CheckKeyword(1, 4, "uper", TokenType.Super);
            case 't':
                if (this.current - this.start > 1)
                {
                    switch (this.source[this.start + 1])
                    {
                        case 'h': return CheckKeyword(2, 2, "is", TokenType.This);
                        case 'r': return CheckKeyword(2, 2, "ue", TokenType.True);
                    }
                }
                break;
            case 'v': return CheckKeyword(1, 2, "ar", TokenType.Var);
            case 'w': return CheckKeyword(1, 4, "hile", TokenType.While);
        }
        return TokenType.Identifier;
    }
    private TokenType CheckKeyword(int start, int length, string rest, TokenType type) => this.current - this.start == start + length && this.source[(this.start + start)..(this.start + start + length)] == rest ? type : TokenType.Identifier;

    private Token Identifier()
    {
        while (IsAlpha(this.Peek()) || IsDigit(this.Peek()))
            this.Advance();
        return this.MakeToken(this.IdentifierType());
    }

    private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private Token Number()
    {
        while (IsDigit(this.Peek()))
            this.Advance();

        if (this.Peek() == '.' && IsDigit(this.PeekNext()))
        {
            this.Advance();

            while (IsDigit(this.Peek()))
                this.Advance();
        }

        return MakeToken(TokenType.Number);
    }

    private Token String()
    {
        while (!this.IsAtEnd && this.Peek() != '"')
        {
            if (this.Peek() == '\n')
                this.line++;
            this.Advance();
        }

        if (this.IsAtEnd)
            return ErrorToken("Unterminated string");

        this.Advance();
        return MakeToken(TokenType.String);
    }

    private void SkipWhitespace()
    {
        for (; !IsAtEnd;)
        {
            var c = this.Peek();
            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    this.Advance();
                    break;
                case '\n':
                    this.line++;
                    this.Advance();
                    break;
                case '/':
                    if (this.PeekNext() == '/')
                        while (!IsAtEnd && this.Peek() != '\n')
                            this.Advance();
                    else
                        return;
                    break;
                default:
                    return;
            }
        }
    }

    private char PeekNext() => this.IsAtEnd ? '\0' : this.source[this.current + 1];

    private char Peek() => this.source[this.current];

    private bool Match(char c)
    {
        if (this.IsAtEnd || this.Peek() != c)
            return false;
        this.current++;
        return true;
    }

    private char Advance() => this.source[this.current++];

    public bool IsAtEnd => this.current == source.Length - 1;

    private Token MakeToken(TokenType type) => new(type, this.source, this.start, this.current - this.start, this.line);

    private Token ErrorToken(string message) =>
        new(TokenType.Error, message, 0, message.Length, this.line);
}
