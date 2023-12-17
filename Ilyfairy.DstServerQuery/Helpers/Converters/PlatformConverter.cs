using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters;

public class PlatformConverter : JsonConverter<Platform>
{
    public override Platform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Platform)reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, Platform value, JsonSerializerOptions options)
    {
        //var str = DstEnumText.Instance.TryGetValueOrDefault(value, "未知");
        writer.WriteStringValue(value.ToString());
    }
}