namespace NLox.Runtime;

using System.Runtime.InteropServices;

internal static class Memory
{
    public static int GrowCapacity(int capacity) => capacity < 4 ? 4 : capacity << 1;

    public static unsafe T* GrowArray<T>(T* pointer, nuint oldCount, nuint newCount) where T : unmanaged =>
        (T*)Reallocate(pointer, (nuint)sizeof(T) * oldCount, (nuint)sizeof(T) * newCount);

    public static unsafe void* Reallocate(void* pointer, nuint oldSize, nuint newSize)
    {
        if (newSize == 0)
        {
            NativeMemory.Free(pointer);
            return null;
        }

        var result = NativeMemory.Realloc(pointer, newSize);
        return result;
    }
}