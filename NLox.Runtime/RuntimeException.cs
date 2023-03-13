#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

[Serializable]
public class RuntimeException : NloxException
{
    public RuntimeException() { }
    public RuntimeException(string message) : base(message) { }
    public RuntimeException(string message, Exception inner) : base(message, inner) { }
    protected RuntimeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


[Serializable]
public class CompileException : NloxException
{
    public CompileException() { }
    public CompileException(string message) : base(message) { }
    public CompileException(string message, Exception inner) : base(message, inner) { }
    protected CompileException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}