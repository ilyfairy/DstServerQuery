using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;


public class WorldLevelRawConverter : JsonConverter<LobbyWorldLevel>
{
    public override LobbyWorldLevel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<LobbyWorldLevel>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, LobbyWorldLevel value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}

public class WorldLevelConverter : JsonConverter<LobbyWorldLevel>
{
    public override LobbyWorldLevel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //while (reader.Read())
        //{
            
        //}

        //var levels = JsonSerializer.Deserialize<IWorldLevelItem[]>(ref reader);
        //if (levels is null) return null;

        LobbyWorldLevel lobbyWorldLevels = new();
        //lobbyWorldLevels.Levels = levels.ToDictionary(v => v.Id);

        return lobbyWorldLevels;
    }

    public override void Write(Utf8JsonWriter writer, LobbyWorldLevel value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Values.AsEnumerable<IWorldLevelItem>());
    }
}