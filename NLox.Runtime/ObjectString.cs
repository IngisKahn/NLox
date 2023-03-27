namespace NLox.Runtime;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ObjectString
{
    public ObjectType Type;
    public int Length;
    public byte* Chars;
    public uint Hash;

    public static ObjectString* CopyString(byte* chars, int length, Action<IntPtr> registerObject)
    {
        var hash = HashString(chars, length);
        var heapChars = Memory.Allocate<byte>((nuint)length);
        NativeMemory.Copy(chars, heapChars, (nuint)length);
        return AllocateString(heapChars, length, hash, registerObject);
    }
    public static ObjectString* TakeString(byte* chars, int length, Action<IntPtr> registerObject) =>
        AllocateString(chars, length, HashString(chars, length), registerObject);
    public static ObjectString* AllocateString(byte* chars, int length, uint hash, Action<IntPtr> registerObject)
    {
        var s = Object.AllocateObject<ObjectString>(ObjectType.String, registerObject);
        s->Length = length;
        s->Chars = chars;
        s->Hash = hash;
        return s;
    }

    public static uint HashString(byte* key, int length)
    {
        var hash = 2166136261u;
        for (var i = 0; i < length; i++)
        {
            hash ^= key[i];
            hash *= 16777619;
        }
        return hash;
    }
}