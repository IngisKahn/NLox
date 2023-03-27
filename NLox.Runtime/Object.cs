using System.Runtime.InteropServices;

namespace NLox.Runtime;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Object
{
    public Object* Next;
    public ObjectType Type;

    public static T* AllocateObject<T>(ObjectType type, Action<IntPtr> registerObject) where T : unmanaged
    {
        var o = (Object*)Memory.Reallocate(null, 0, (nuint)sizeof(T));
        o->Type = type;
        registerObject((IntPtr)o);
        return (T*)o;
    }

    public static void Free(Object* o)
    {
        switch (o->Type)
        {
            case ObjectType.String:
            {
                var s = (ObjectString*)o;
                Memory.FreeArray(s->Chars, (nuint)s->Length);
                Memory.Free(o);
            }
                break;
        }
    }
}