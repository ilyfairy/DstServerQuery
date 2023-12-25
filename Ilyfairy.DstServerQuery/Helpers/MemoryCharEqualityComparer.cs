using System.Diagnostics.CodeAnalysis;

namespace Ilyfairy.DstServerQuery.Helpers.Converters.Cache;

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