﻿namespace NLox.Interpreter;

public enum TokenType
{
    // Single-character tokens.
    LeftParen, RightParen, LeftBrace, RightBrace,
    Comma, Colon, Dot, Minus, Plus, Question, Semicolon, Slash, Star,

    // One or two character tokens.
    Bang, BangEqual,
    Equal, EqualEqual,
    Greater, GreaterEqual,
    Less, LessEqual,

    // Literals.
    Identifier, String, Number,

    // Keywords.
    And, Break, Class, Continue, Else, False, Fun, For, If, Nil, Or,
    Print, Return, Super, This, True, Var, While,

    EoF
}