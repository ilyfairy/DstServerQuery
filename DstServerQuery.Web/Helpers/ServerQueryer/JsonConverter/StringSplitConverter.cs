using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer.JsonConverters;

public class StringSplitConverter : JsonConverter<string?[]>
{
    public static char[] SplitChars { get; } = [';', '|', ','];

    public static StringSplitConverter Instance { get; } = new();

    public override string?[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString()!;
            return str.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();
        }
        else if(reader.TokenType == JsonTokenType.StartArray)
        {
            List<string?> arr = new();
            while (true)
            {
                if (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        arr.Add(reader.GetString());
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        arr.Add(reader.GetInt64().ToString());
                    }
                }
            }
            return arr.ToArray();
        }
        else
        {
            return JsonSerializer.Deserialize<string[]>(ref reader);
        }
    }

    public override void Write(Utf8JsonWriter writer, string?[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}