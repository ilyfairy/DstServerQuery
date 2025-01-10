using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Models.Lobby;

namespace DstServerQuery.Converters;

internal class LobbySteamIdConverter : JsonConverter<LobbySteamId>
{
    public override LobbySteamId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        // 90243943705555997
        // P:48001167358525925
        // N:bVUBL4btQjQBAAAAAAAAAAE

        var valueSpan = reader.ValueSpan;
        if (valueSpan.IndexOf((byte)':') is int index and not -1)
        {
            valueSpan = valueSpan[(index + 1)..];
        }
        return new LobbySteamId(valueSpan);
    }

    public override void Write(Utf8JsonWriter writer, LobbySteamId value, JsonSerializerOptions options)
    {
        if (value.String is not null)
        {
            writer.WriteStringValue(value.String);
            return;
        }
        Span<byte> output = stackalloc byte[20];
        value.Value.TryFormat(output, out var writtenLen);
        writer.WriteStringValue(output[..writtenLen]); // "value.Value"
    }
}
