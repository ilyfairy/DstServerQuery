using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class IntentRawCacheConverter : JsonConverter<string>
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
            _ when reader.ValueSpan.SequenceEqual("relaxed"u8) => "relaxed",
            _ when reader.ValueSpan.SequenceEqual("endless"u8) => "endless",
            _ when reader.ValueSpan.SequenceEqual("survival"u8) => "survival",
            _ when reader.ValueSpan.SequenceEqual("wilderness"u8) => "wilderness",
            _ when reader.ValueSpan.SequenceEqual("cooperative"u8) => "cooperative",
            _ when reader.ValueSpan.SequenceEqual("lightsout"u8) => "lightsout",
            _ when reader.ValueSpan.SequenceEqual("oceanfishing"u8) => "oceanfishing",
            _ => reader.GetString(),

        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
