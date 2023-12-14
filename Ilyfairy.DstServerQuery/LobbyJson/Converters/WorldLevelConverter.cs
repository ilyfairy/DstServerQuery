using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class WorldLevelConverter : JsonConverter<ILobbyWorldLevel[]>
{
    public override ILobbyWorldLevel[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dic = JsonSerializer.Deserialize<Dictionary<string, LobbyWorldLevel>>(ref reader);
        return dic?.Values.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, ILobbyWorldLevel[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
