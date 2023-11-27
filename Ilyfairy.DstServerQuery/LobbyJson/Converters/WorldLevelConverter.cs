using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class WorldLevelConverter : JsonConverter<ILobbyWorldLevel>
{
    public override ILobbyWorldLevel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<LobbyWorldLevel>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, ILobbyWorldLevel value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<ILobbyWorldLevel>(writer, value);
    }
}
