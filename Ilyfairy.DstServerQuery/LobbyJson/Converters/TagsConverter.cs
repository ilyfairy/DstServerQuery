using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class TagsConverter : JsonConverter<string[]>
{
    
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.GetString() is not string str) return Array.Empty<string>();
        var tags = str.Split(',', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tags.Length; i++)
        {
            tags[i] = string.Intern(tags[i]);
        }
        return tags;
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
