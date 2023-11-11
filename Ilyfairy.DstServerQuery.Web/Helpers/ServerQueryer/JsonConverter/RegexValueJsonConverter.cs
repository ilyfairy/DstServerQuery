using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer.JsonConverters;

public class RegexValueJsonConverter : JsonConverter<RegexValue>
{
    public override RegexValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new RegexValue();
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            return new RegexValue()
            {
                Value = reader.GetString(),
            };
        }
        else
        {
            return JsonSerializer.Deserialize<RegexValue>(ref reader);
        }
    }

    public override void Write(Utf8JsonWriter writer, RegexValue value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
