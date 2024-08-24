using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Web.Helpers.ServerQueryer.JsonConverter;

public class StringArrayJsonConverter : JsonConverter<StringArray>
{
    public override StringArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new StringArray();
        }
        else if (reader.TokenType is JsonTokenType.String or JsonTokenType.StartArray)
        {
            return new()
            {
                Value = StringSplitConverter.Instance.Read(ref reader, typeof(string[]), options)
            };
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return new()
            {
                Value = [reader.GetInt64().ToString()]
            };
        }
        else
        {
            return JsonSerializer.Deserialize<StringArray>(ref reader, new JsonSerializerOptions()
            {

            });
        }
    }

    public override void Write(Utf8JsonWriter writer, StringArray value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
