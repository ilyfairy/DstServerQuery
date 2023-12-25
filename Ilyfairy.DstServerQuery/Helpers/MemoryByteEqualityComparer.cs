using System.Diagnostics.CodeAnalysis;

namespace Ilyfairy.DstServerQuery.Helpers;

public class MemoryByteEqualityComparer : IEqualityComparer<ReadOnlyMemory<byte>>
{
    public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode([DisallowNull] ReadOnlyMemory<byte> obj)
    {
        return Utils.GetHashCodeFast(obj.Span);
    }
}