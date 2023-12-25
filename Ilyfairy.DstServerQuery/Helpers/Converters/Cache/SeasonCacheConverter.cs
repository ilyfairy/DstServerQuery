using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters.Cache;

public class SeasonCacheConverter : JsonConverter<string>
{
    //private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();
    //public static ConcurrentDictionary<ReadOnlyMemory<byte>, string> Cache { get; } = new(new MemoryByteEqualityComparer());

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return string.Empty;

        if (reader.ValueSpan.SequenceEqual("spring"u8))
        {
            return "spring";
        }
        else if (reader.ValueSpan.SequenceEqual("summer"u8))
        {
            return "summer";
        }
        else if (reader.ValueSpan.SequenceEqual("autumn"u8))
        {
            return "autumn";
        }
        else if (reader.ValueSpan.SequenceEqual("winter"u8))
        {
            return "winter";
        }

        return reader.GetString();

        //var temp = pool.Rent(reader.ValueSpan.Length);
        //var memory = temp.AsMemory(0, reader.ValueSpan.Length);
        //reader.ValueSpan.CopyTo(temp);
        //if (Cache.TryGetValue(memory, out var str))
        //{
        //    pool.Return(temp);
        //    return str;
        //}
        //else
        //{
        //    pool.Return(temp);
        //    str = reader.GetString()!;
        //    Cache[reader.ValueSpan.ToArray()] = str;
        //    return str;
        //}
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
