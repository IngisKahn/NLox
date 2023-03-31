namespace NLox.Runtime;
public enum OpCode : byte
{
    Constant,
    Nil,
    True,
    False,
    DefineGlobal,
    Equal,
    GetGlobal,
    GetLocal,
    Greater,
    Less,
    Add,
    Subtract,
    Multiply,
    Divide,
    Not,
    Negate,
    Pop,
    Print,
    Return,
    SetGlobal,
    SetLocal
}
