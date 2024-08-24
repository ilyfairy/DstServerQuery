using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters;

public class ReadOnlyMemoryCharJsonConverter : JsonConverter<ReadOnlyMemory<char>>
{
    public override ReadOnlyMemory<char> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString().AsMemory();
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<char> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Span);
    }
}