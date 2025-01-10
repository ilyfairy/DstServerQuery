using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class IntentConverter : JsonConverter<LobbyIntent>
{
    public override LobbyIntent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return new(string.Empty);

        return 0 switch
        {
            _ when reader.ValueSpan.SequenceEqual("relaxed"u8) => new("relaxed"),
            _ when reader.ValueSpan.SequenceEqual("endless"u8) => new("endless"),
            _ when reader.ValueSpan.SequenceEqual("survival"u8) => new("survival"),
            _ when reader.ValueSpan.SequenceEqual("wilderness"u8) => new("wilderness"),
            _ when reader.ValueSpan.SequenceEqual("cooperative"u8) => new("cooperative"),
            _ when reader.ValueSpan.SequenceEqual("lightsout"u8) => new("lightsout"),
            _ when reader.ValueSpan.SequenceEqual("oceanfishing"u8) => new("oceanfishing"),
            _ => new(reader.GetString()),
        };
    }

    public override void Write(Utf8JsonWriter writer, LobbyIntent value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class IntentWithTranslateConverter : JsonConverter<LobbyIntent>
{
    public override LobbyIntent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, LobbyIntent value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}