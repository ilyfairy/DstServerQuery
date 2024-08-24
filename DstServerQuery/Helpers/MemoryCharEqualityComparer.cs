using System.Diagnostics.CodeAnalysis;

namespace DstServerQuery.Helpers;

public class MemoryCharEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode([DisallowNull] ReadOnlyMemory<char> obj)
    {
        return Utils.GetHashCodeFast(obj.Span);
    }
}