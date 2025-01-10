using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class LobbyTagsConverter : JsonConverter<string[]>
{
    public static Dictionary<string, string> Cache { get; } = new();
    public static Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> CacheAlternateLookup { get; }

    static LobbyTagsConverter()
    {
        CacheAlternateLookup = Cache.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var charsCount = Encoding.UTF8.GetCharCount(reader.ValueSpan);
        char[]? sharedBuffer = null;
        Span<char> buffer = charsCount < 512 ? stackalloc char[charsCount] :
            (sharedBuffer = ArrayPool<char>.Shared.Rent(charsCount));

        try
        {
            Encoding.UTF8.GetChars(reader.ValueSpan, buffer);
            var tagsCount = ((ReadOnlySpan<char>)buffer).Count(',');
            var tags = new string[tagsCount + 1];
            int index = 0;
            foreach (var item in ((ReadOnlySpan<char>)buffer).Split(','))
            {
                if (CacheAlternateLookup.TryGetValue(buffer[item], out var tag))
                {
                    tags[index] = tag;
                }
                else
                {
                    tag = buffer[item].ToString();
                    Cache[tag] = tag;
                    tags[index] = tag;
                }
                index++;
            }
            return tags;
        }
        finally
        {
            if (sharedBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(sharedBuffer);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}
