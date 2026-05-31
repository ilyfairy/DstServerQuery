using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstDownloaders.Converters;

public class StringCacheConverter : JsonConverter<string>
{
    public ConcurrentDictionary<string, string> Cache { get; } = new();
    public ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> CacheAlternateLookup { get; }

    public StringCacheConverter()
    {
        CacheAlternateLookup = Cache.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("不是一个字符串");
        }

        if (reader.ValueSpan.Length < 30)
        {
            Span<char> buffer = stackalloc char[30];
            var len = reader.CopyString(buffer);
            if (CacheAlternateLookup.TryGetValue(buffer[..len], out var str))
            {
                return str;
            }
            else
            {
                str = new string(buffer[..len]);
                Cache.TryAdd(str, str);
                return str;
            }
        }
        else
        {
            var charsCount = Encoding.UTF8.GetCharCount(reader.ValueSpan);
            var buffer = ArrayPool<char>.Shared.Rent(charsCount);
            try
            {
                var len = reader.CopyString(buffer);
                if (CacheAlternateLookup.TryGetValue(buffer.AsSpan(0, len), out var str))
                {
                    return str;
                }
                else
                {
                    str = new string(buffer.AsSpan(0, len));
                    Cache.TryAdd(str, str);
                    return str;
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
