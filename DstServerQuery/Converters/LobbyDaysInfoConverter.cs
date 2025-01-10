using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Helpers;
using DstServerQuery.Models;

namespace DstServerQuery.Converters;

public class LobbyDaysInfoConverter : JsonConverter<LobbyDaysInfo>
{
    public override LobbyDaysInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new Exception("不是一个字符串");
        }
        if (reader.ValueSpan.Length <= 128)
        {
            Span<char> buffer = stackalloc char[reader.ValueSpan.Length];
            var charsLen = reader.CopyString(buffer);
            if (DstConverterHelper.ParseDays(buffer[..charsLen]) is { } days)
            {
                return days;
            }
            else
            {
                using TempUtf8JsonString str = TempUtf8JsonString.From(reader);
                return DstConverterHelper.ParseDays(str.String);
            }
        }
        else
        {
            using TempUtf8JsonString str = TempUtf8JsonString.From(reader);
            if (DstConverterHelper.ParseDays(str.String.Span) is { } days)
            {
                return days;
            }
            else
            {
                return DstConverterHelper.ParseDays(str.String);
            }
        }
    }
    public override void Write(Utf8JsonWriter writer, LobbyDaysInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
