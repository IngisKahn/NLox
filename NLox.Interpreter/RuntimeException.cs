namespace NLox.Interpreter;

using System.Diagnostics.CodeAnalysis;

[Serializable]
public class RuntimeException : Exception
{
    public Token Token { get; }

    public RuntimeException(Token token, string message) : base(message) => this.Token = token;
    public RuntimeException(Token token, string message, Exception inner) : base(message, inner) => this.Token = token;
    protected RuntimeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
