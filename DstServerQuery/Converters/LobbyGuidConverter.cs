using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Models.Lobby;

namespace DstServerQuery.Converters;

internal class LobbyGuidConverter : JsonConverter<LobbyGuid>
{
    public override LobbyGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        // 8451361192076405236

        Debug.Assert(reader.ValueSpan.Length <= 20);
        return new LobbyGuid(reader.ValueSpan);
    }

    public override void Write(Utf8JsonWriter writer, LobbyGuid value, JsonSerializerOptions options)
    {
        Span<byte> output = stackalloc byte[20];
        value.Value.TryFormat(output, out var writtenLen);
        writer.WriteStringValue(output[..writtenLen]);
    }
}
