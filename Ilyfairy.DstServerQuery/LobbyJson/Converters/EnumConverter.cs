using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class EnumConverter<T> : JsonConverter<T> where T : Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (Enum.TryParse(typeToConvert, str, true, out var obj))
            {
                return (T)obj;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return (T)Enum.ToObject(typeToConvert, reader.GetInt64());
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
