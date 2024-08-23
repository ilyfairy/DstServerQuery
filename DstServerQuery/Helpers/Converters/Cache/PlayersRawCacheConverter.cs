using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters.Cache;

public class PlayersRawCacheConverter : JsonConverter<string>
{
    private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();
    public static ConcurrentDictionary<ReadOnlyMemory<byte>, string> Cache { get; } = new(new MemoryByteEqualityComparer());
    private static ReadOnlySpan<byte> emptyUtf8 => "return {  }"u8;
    private static readonly string empty = "return {  }";

    static PlayersRawCacheConverter()
    {
        string[] caches = [empty];
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

        if (reader.ValueSpan.SequenceEqual(emptyUtf8))
        {
            return empty;
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
        //    //Cache[reader.ValueSpan.ToArray()] = str;
        //    return str;
        //}
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
