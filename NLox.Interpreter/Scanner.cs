namespace NLox.Interpreter;

using System;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new();
    private readonly Func<int, string, Task>? error;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        ["and"] = TokenType.And,
        ["class"] = TokenType.Class,
        ["else"] = TokenType.Else,
        ["false"] = TokenType.False,
        ["for"] = TokenType.For,
        ["fun"] = TokenType.Fun,
        ["if"] = TokenType.If,
        ["nil"] = TokenType.Nil,
        ["or"] = TokenType.Or,
        ["print"] = TokenType.Print,
        ["return"] = TokenType.Return,
        ["super"] = TokenType.Super,
        ["this"] = TokenType.This,
        ["true"] = TokenType.True,
        ["var"] = TokenType.Var,
        ["while"] = TokenType.While
    };

    public Scanner(string source, Func<int, string, Task>? error = null)
    {
        this.source = source;
        this.error = error;
    }

    public async Task<IList<Token>> ScanTokens()
    {
        while (!this.IsAtEnd)
        {
            // We are at the beginning of the next lexeme.
            start = current;
            await this.ScanToken();
        }

        tokens.Add(new(TokenType.EoF, string.Empty, null, line));
        return tokens;
    }

    private bool IsAtEnd => this.current >= this.source.Length;

    private async Task ScanToken()
    {
        var c = this.Advance();
        switch (c)
        {
            case '(': this.AddToken(TokenType.LeftParen); break;
            case ')': this.AddToken(TokenType.RightParen); break;
            case '{': this.AddToken(TokenType.LeftBrace); break;
            case '}': this.AddToken(TokenType.RightBrace); break;
            case ',': this.AddToken(TokenType.Comma); break;
            case ':': this.AddToken(TokenType.Colon); break;
            case '.': this.AddToken(TokenType.Dot); break;
            case '-': this.AddToken(TokenType.Minus); break;
            case '+': this.AddToken(TokenType.Plus); break;
            case '?': this.AddToken(TokenType.Question); break;
            case ';': this.AddToken(TokenType.Semicolon); break;
            case '*': this.AddToken(TokenType.Star); break;
            case '!':
                this.AddToken(this.Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                this.AddToken(this.Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                this.AddToken(this.Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                this.AddToken(this.Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (this.Match('/'))
                    // A comment goes until the end of the line.
                    while (this.Peek != '\n' && !this.IsAtEnd)
                        this.Advance();
                else
                    this.AddToken(TokenType.Slash);
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                line++;
                break;
            case '"':
                await this.String();
                break;
            default:
                if (Scanner.IsDigit(c))
                    this.Number();
                else if (Scanner.IsAlpha(c))
                    this.Identifier();
                else
                    await (this.error?.Invoke(this.line, "Unexpected character") ?? Task.CompletedTask);
                break;
        }
    }

    private char Advance() => this.source[this.current++];

    private void AddToken(TokenType token) => this.AddToken(token, null);

    private void AddToken(TokenType type, object? literal)
    {
        var text = source[start..current];
        tokens.Add(new(type, text, literal, line));
    }
    private bool Match(char expected)
    {
        if (this.IsAtEnd)
            return false;
        if (this.source[this.current] != expected)
            return false;

        current++;
        return true;
    }
    private char Peek => this.IsAtEnd ? '\0' : source[current];

    private async Task String()
    {
        while (this.Peek != '"' && !this.IsAtEnd)
        {
            if (this.Peek == '\n')
                line++;
            this.Advance();
        }

        if (this.IsAtEnd)
        {
            await (this.error?.Invoke(this.line, "Unterminated string.") ?? Task.CompletedTask);
            return;
        }

        // The closing ".
        this.Advance();

        // Trim the surrounding quotes.
        var value = source[(start + 1)..(current - 1)];
        this.AddToken(TokenType.String, value);
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private void Number()
    {
        while (Scanner.IsDigit(this.Peek))
            this.Advance();

        // Look for a fractional part.
        if (this.Peek == '.' && Scanner.IsDigit(this.PeekNext))
        {
            // Consume the "."
            this.Advance();

            while (Scanner.IsDigit(this.Peek))
                this.Advance();
        }

        this.AddToken(TokenType.Number,
            double.Parse(this.source[start..current]));
    }
    private char PeekNext => this.current + 1 >= source.Length ? '\0' : source[current + 1];

    private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or
               >= 'A' and <= 'Z' or
                '_';

    private static bool IsAlphaNumeric(char c) => Scanner.IsAlpha(c) || Scanner.IsDigit(c);

    private void Identifier()
    {
        while (IsAlphaNumeric(this.Peek))
            this.Advance();
        var text = source[start..current];
        if (!keywords.TryGetValue(text, out var type))
            type = TokenType.Identifier;
        this.AddToken(type);
    }
}
