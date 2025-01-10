using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Models.Lobby;

namespace DstServerQuery.Converters;

public class LobbyNumberIdConverter : JsonConverter<LobbyNumberId>
{
    public override LobbyNumberId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("不是一个字符串");
        }

        if (long.TryParse(reader.ValueSpan, out var number))
        {
            return new LobbyNumberId(number);
        }
        else
        {
            throw new JsonException("不是一个数字");
        }
    }
    public override void Write(Utf8JsonWriter writer, LobbyNumberId value, JsonSerializerOptions options)
    {
        Span<byte> output = stackalloc byte[20];
        value.Value.TryFormat(output, out var writtenLen);
        writer.WriteStringValue(output[..writtenLen]);
    }
}
