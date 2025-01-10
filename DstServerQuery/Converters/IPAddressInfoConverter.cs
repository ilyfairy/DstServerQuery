using DstServerQuery.Helpers;
using DstServerQuery.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class IPAddressStringConverter : JsonConverter<IPAddressInfo>
{
    public override void Write(Utf8JsonWriter writer, IPAddressInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.IP);
    }

    public override IPAddressInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType != JsonTokenType.Null);

        if (reader.ValueSpan.SequenceEqual("127.0.0.1"u8))
        {
            return DstConverterHelper.ParseAddress("127.0.0.1");
        }

        return DstConverterHelper.ParseAddress(reader.GetString()!);
    }
}