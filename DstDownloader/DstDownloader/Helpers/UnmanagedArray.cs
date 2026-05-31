using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// AOT 正常
public sealed unsafe class UnmanagedArray<T> : IDisposable where T : unmanaged
{
    private readonly void* _ptr;

    private readonly T[] _array;
    public T[] Array => _array;

    public UnmanagedArray(int length)
    {
        int arraySize = (nint.Size * 2) + 8 + (sizeof(T) * length);
        _ptr = NativeMemory.AllocZeroed((nuint)arraySize);

        *(nint*)((byte*)_ptr + nint.Size) = typeof(T[]).TypeHandle.Value;
        *(int*)((byte*)_ptr + nint.Size * 2) = length;

        var objectPtr = (byte*)_ptr + nint.Size;
        var array = Unsafe.Read<T[]>(&objectPtr);
        _array = array;
    }


    public void Dispose()
    {
        NativeMemory.Free(_ptr);
    }
}
