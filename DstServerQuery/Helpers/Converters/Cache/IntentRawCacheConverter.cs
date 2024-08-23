using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters.Cache;

public class IntentRawCacheConverter : JsonConverter<string>
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

        if (reader.ValueSpan.SequenceEqual("relaxed"u8))
        {
            return "relaxed";
        }
        else if (reader.ValueSpan.SequenceEqual("endless"u8))
        {
            return "endless";
        }
        else if (reader.ValueSpan.SequenceEqual("survival"u8))
        {
            return "survival";
        }
        else if (reader.ValueSpan.SequenceEqual("wilderness"u8))
        {
            return "wilderness";
        }
        else if (reader.ValueSpan.SequenceEqual("cooperative"u8))
        {
            return "cooperative";
        }
        else if (reader.ValueSpan.SequenceEqual("lightsout"u8))
        {
            return "lightsout";
        }
        else if (reader.ValueSpan.SequenceEqual("oceanfishing"u8))
        {
            return "oceanfishing";
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
