using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class PlayersRawCacheConverter : JsonConverter<string>
{
    public static ConcurrentDictionary<ReadOnlyMemory<byte>, string> Cache { get; } = new(new MemoryByteEqualityComparer());
    private static ReadOnlySpan<byte> _emptyUtf8 => "return {  }"u8;
    private static readonly string _empty = "return {  }";

    static PlayersRawCacheConverter()
    {
        string[] caches = [_empty];
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

        if (reader.ValueSpan.SequenceEqual(_emptyUtf8))
        {
            return _empty;
        }

        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
