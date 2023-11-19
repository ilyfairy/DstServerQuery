using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class WorldLevelConverter : JsonConverter<Dictionary<string, LobbyWorldLevel>>
{
    public override Dictionary<string, LobbyWorldLevel>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Dictionary<string, LobbyWorldLevel>>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, LobbyWorldLevel> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var (k, v) in value)
        {
            writer.WritePropertyName(k);
            JsonSerializer.Serialize<ILobbyWorldLevel>(writer, v);
        }
        writer.WriteEndObject();
    }
}
