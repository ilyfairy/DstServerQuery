using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Ilyfairy.DstServerQuery.Web.Helpers;

public class DstPlayerEqualityComparer : IEqualityComparer<DstPlayer>
{
    public static DstPlayerEqualityComparer Instance { get; } = new DstPlayerEqualityComparer();

    public bool Equals(DstPlayer? x, DstPlayer? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] DstPlayer obj) => obj.Id.GetHashCode();
}
