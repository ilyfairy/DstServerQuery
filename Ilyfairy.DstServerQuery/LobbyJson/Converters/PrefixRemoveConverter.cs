using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converters;

public class PrefixRemoveConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (str is null) return null;
        var perfixIndex = str.IndexOf(':');
        if (perfixIndex != -1)
        {
            return str[(perfixIndex + 1)..];
        }
        return str;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
