using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby;

namespace DstServerQuery.Converters;

public class LobbySessionIdConverter : JsonConverter<LobbySessionId>
{
    public override LobbySessionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        // "0E516F29EF6D3712"
        // "05375D373EC81FY3"

        Debug.Assert(reader.ValueSpan.Length == 16);
        return new LobbySessionId(reader.ValueSpan);
    }

    public override void Write(Utf8JsonWriter writer, LobbySessionId value, JsonSerializerOptions options)
    {
        Span<char> hex = stackalloc char[value.Value.Length];
        for (var i = 0; i < value.Value.Length; i++)
        {
            byte b = value.Value[i];
            hex[i] = (char)b;
        }
        writer.WriteStringValue(hex);
    }
}
