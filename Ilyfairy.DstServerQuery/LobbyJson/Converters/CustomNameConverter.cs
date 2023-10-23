using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converters;

/// <summary>
/// 自定义名称序列化转换器, 仅允许序列化
/// </summary>
/// <typeparam name="T"></typeparam>
public class CustomNameConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var item in typeof(T).GetProperties())
        {
            writer.WritePropertyName(item.Name);
            JsonSerializer.Serialize(writer, item.GetValue(value), item.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}
