using System.Text.Json;
using System.Text.Json.Serialization;
using DstServerQuery.Models;

namespace DstServerQuery.Converters;

public class LobbyWorldLevelConverter : JsonConverter<WorldLevelItem[]>
{
    public override WorldLevelItem[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var r = JsonSerializer.Deserialize<Dictionary<string, WorldLevelRawItem>>(ref reader);
        return r?.Values?.Select(v => WorldLevelItem.FromRaw(v)).ToArray();
    }
    public override void Write(Utf8JsonWriter writer, WorldLevelItem[] value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
