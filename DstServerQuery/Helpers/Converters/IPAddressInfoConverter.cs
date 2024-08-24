using DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters;

public class IPAddressStringConverter : JsonConverter<IPAddressInfo>
{
    public override void Write(Utf8JsonWriter writer, IPAddressInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.IP);
    }

    public override IPAddressInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}