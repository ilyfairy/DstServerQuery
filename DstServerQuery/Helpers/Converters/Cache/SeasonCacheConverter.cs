using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class SeasonCacheConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return string.Empty;

        return 0 switch
        {
            _ when reader.ValueSpan.SequenceEqual("spring"u8) => "spring",
            _ when reader.ValueSpan.SequenceEqual("summer"u8) => "summer",
            _ when reader.ValueSpan.SequenceEqual("autumn"u8) => "autumn",
            _ when reader.ValueSpan.SequenceEqual("winter"u8) => "winter",
            _ => reader.GetString(),
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
