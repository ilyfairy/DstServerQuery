using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters.Cache;

public class ModsInfoRawCacheConverter : JsonConverter<object[]>
{
    private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();
    public static ConcurrentDictionary<ReadOnlyMemory<byte>, string> Cache { get; } = new(new MemoryByteEqualityComparer());


    public override object[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("不是一个数组");

        List<object?> list = new(8);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var temp = pool.Rent(reader.ValueSpan.Length);
                var memory = temp.AsMemory(0, reader.ValueSpan.Length);
                reader.ValueSpan.CopyTo(temp);
                if (Cache.TryGetValue(memory, out var str))
                {
                    pool.Return(temp);
                    list.Add(str);
                }
                else
                {
                    pool.Return(temp);
                    str = reader.GetString()!;
                    Cache[reader.ValueSpan.ToArray()] = str;
                    list.Add(str);
                }
            }
            else
            {
                list.Add(reader.TokenType switch
                {
                    JsonTokenType.Null => null,
                    JsonTokenType.Number => reader.GetUInt32(),
                    JsonTokenType.True => true,
                    JsonTokenType.False => true,
                    _ => throw new JsonException("不是基元类型")
                });
            }
        }

        return list.ToArray()!;
    }

    public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
