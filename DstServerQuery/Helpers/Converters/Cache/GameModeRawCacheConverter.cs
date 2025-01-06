using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class GameModeRawCacheConverter : JsonConverter<string>
{
    private static ConcurrentUtf8CharCacheDictionary _cache = new();

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return string.Empty;

        return _cache.GetOrAdd(reader.ValueSpan);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
