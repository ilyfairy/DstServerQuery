using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class LobbySeasonConverter : JsonConverter<LobbySeason>
{
    public override LobbySeason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return new(string.Empty);

        return 0 switch
        {
            _ when reader.ValueSpan.SequenceEqual("spring"u8) => new("spring"),
            _ when reader.ValueSpan.SequenceEqual("summer"u8) => new("summer"),
            _ when reader.ValueSpan.SequenceEqual("autumn"u8) => new("autumn"),
            _ when reader.ValueSpan.SequenceEqual("winter"u8) => new("winter"),
            _ => new(reader.GetString()),
        };
    }
    public override void Write(Utf8JsonWriter writer, LobbySeason value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class SeasonWithTranslateConverter : JsonConverter<LobbySeason>
{
    public override LobbySeason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
    public override void Write(Utf8JsonWriter writer, LobbySeason value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}