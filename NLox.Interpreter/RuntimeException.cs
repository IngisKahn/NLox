namespace NLox.Interpreter;

using System.Diagnostics.CodeAnalysis;

[Serializable]
public class RuntimeException : Exception
{
    public Token Token { get; }

    public RuntimeException(Token token, string message) : base(message) => this.Token = token;
    public RuntimeException(Token token, string message, Exception inner) : base(message, inner) => this.Token = token;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected RuntimeException(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
