using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters.Cache;

public class IdRawCacheConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("不是一个string");
        }

        if (reader.ValueSpan.Length == 0)
            return string.Empty;

        if (reader.ValueSpan.Length == 1)
        {
            return (char)reader.ValueSpan[0] switch
            {
                '0' => "0",
                '1' => "1",
                '2' => "2",
                '3' => "3",
                '4' => "4",
                '5' => "5",
                '6' => "6",
                '7' => "7",
                '8' => "8",
                '9' => "9",
                _ => reader.GetString(),
            };
        }
        else if (reader.ValueSpan.Length == 5)
        {
            if (reader.ValueSpan.SequenceEqual("10010"u8))
            {
                return "10010";
            }
        }

        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
