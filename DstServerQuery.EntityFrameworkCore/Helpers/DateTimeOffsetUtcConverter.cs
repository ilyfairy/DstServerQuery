using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DstServerQuery.EntityFrameworkCore.Helpers;

public class DateTimeOffsetUtcConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public static ValueConverterInfo DefaultInfo { get; } = new ValueConverterInfo(typeof(DateTimeOffset), typeof(DateTimeOffset), (i) => new DateTimeOffsetUtcConverter(i.MappingHints));

    public DateTimeOffsetUtcConverter()
        : this(null)
    {
    }

    public DateTimeOffsetUtcConverter(ConverterMappingHints? mappingHints)
        : base((v) => ToUtc(v), (v) => ToLocal(v), mappingHints)
    {
    }

    public static DateTimeOffset ToLocal(DateTimeOffset v)
    {
        return v.ToLocalTime();
    }

    public static DateTimeOffset ToUtc(DateTimeOffset v)
    {
        return v.ToUniversalTime();
    }
}