using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class DaysRawCacheConverter : JsonConverter<string>
{
    private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();
    public static ConcurrentDictionary<ReadOnlyMemory<byte>, string> Cache { get; } = new(new MemoryByteEqualityComparer());

    static DaysRawCacheConverter()
    {
        string[] caches = [
            "return { day=1, dayselapsedinseason=0, daysleftinseason=20 }",
            "return { day=2, dayselapsedinseason=1, daysleftinseason=19 }",
            "return { day=3, dayselapsedinseason=2, daysleftinseason=18 }",
            "return { day=4, dayselapsedinseason=3, daysleftinseason=17 }",
            "return { day=5, dayselapsedinseason=4, daysleftinseason=16 }",
            "return { day=6, dayselapsedinseason=5, daysleftinseason=15 }",
            "return { day=7, dayselapsedinseason=6, daysleftinseason=14 }",
            "return { day=8, dayselapsedinseason=7, daysleftinseason=13 }",
            "return { day=9, dayselapsedinseason=8, daysleftinseason=12 }",
            "return { day=10, dayselapsedinseason=9, daysleftinseason=11 }",
            "return { day=11, dayselapsedinseason=10, daysleftinseason=10 }",
            "return { day=12, dayselapsedinseason=11, daysleftinseason=9 }",
            "return { day=13, dayselapsedinseason=12, daysleftinseason=8 }",
            "return { day=14, dayselapsedinseason=13, daysleftinseason=7 }",
            "return { day=15, dayselapsedinseason=14, daysleftinseason=6 }",
            "return { day=16, dayselapsedinseason=15, daysleftinseason=5 }",
            "return { day=17, dayselapsedinseason=16, daysleftinseason=4 }",
            "return { day=18, dayselapsedinseason=17, daysleftinseason=3 }",
            "return { day=19, dayselapsedinseason=18, daysleftinseason=2 }",
            "return { day=20, dayselapsedinseason=19, daysleftinseason=1 }",
            ];
        foreach (var cache in caches)
        {
            Cache.TryAdd(Encoding.UTF8.GetBytes(cache), cache);
        }
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return string.Empty;

        var temp = pool.Rent(reader.ValueSpan.Length);
        var memory = temp.AsMemory(0, reader.ValueSpan.Length);
        reader.ValueSpan.CopyTo(temp);

        if (Cache.TryGetValue(memory, out var str))
        {
            pool.Return(temp);
            return str;
        }
        else
        {
            pool.Return(temp);
            str = reader.GetString()!;
            //Cache[reader.ValueSpan.ToArray()] = str;
            return str;
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
