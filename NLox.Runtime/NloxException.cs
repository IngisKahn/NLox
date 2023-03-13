#define DEBUG_TRACE_EXECUTION
namespace NLox.Runtime;

[Serializable]
public class NloxException : Exception
{
    public NloxException() { }
    public NloxException(string message) : base(message) { }
    public NloxException(string message, Exception inner) : base(message, inner) { }
    protected NloxException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
