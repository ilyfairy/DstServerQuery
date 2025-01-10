using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class GameModeConverter : JsonConverter<LobbyGameMode>
{
    public static ConcurrentStringCacheDictionary Cache { get; } = new();

    public override LobbyGameMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("不是一个字符串");

        if (reader.ValueSpan.Length == 0)
            return new(string.Empty);

        return new(Cache.GetOrAdd(reader.ValueSpan));
    }

    public override void Write(Utf8JsonWriter writer, LobbyGameMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class GameModeWithTranslateConverter : JsonConverter<LobbyGameMode>
{
    public override LobbyGameMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, LobbyGameMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}