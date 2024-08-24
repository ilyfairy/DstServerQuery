using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class GameModeRawCacheConverter : JsonConverter<string>
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


        if (reader.ValueSpan.SequenceEqual("survival"u8))
        {
            return "survival";
        }
        else if (reader.ValueSpan.SequenceEqual("endless"u8))
        {
            return "endless";
        }
        else if (reader.ValueSpan.SequenceEqual("lavaarena"u8))
        {
            return "lavaarena";
        }
        else if (reader.ValueSpan.SequenceEqual("OceanFishing"u8))
        {
            return "OceanFishing";
        }
        else if (reader.ValueSpan.SequenceEqual("quagmire"u8))
        {
            return "quagmire";
        }
        else if (reader.ValueSpan.SequenceEqual("warsak"u8))
        {
            return "warsak";
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
