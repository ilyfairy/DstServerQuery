using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstDownloaders.Converters;

// string or double or bool
public class LuaObjectJsonConverter : JsonConverter<object>
{
    public static int NumberCacheMaxCount { get; set; } = 1000;
    public static ConcurrentDictionary<double, object> NumberCache { get; } = new();

    public static int StringCacheMaxCharsLength { get; set; } = 30;
    public static int StringCacheMaxCount { get; set; } = 1000;
    public static ConcurrentDictionary<string, string> StringCache { get; } = new();
    private static readonly ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> _stringCacheAlternateLookup;

    private static readonly object _true = true, _false = false;

    static LuaObjectJsonConverter()
    {
        _stringCacheAlternateLookup = StringCache.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => _true,
            JsonTokenType.False => _false,
            JsonTokenType.String => GetOrCacheString(reader),
            JsonTokenType.Number => GetOrCacheNumber(reader),
            _ => null,
        };

        static object GetOrCacheNumber(Utf8JsonReader reader)
        {
            var number = reader.GetDouble();
            if (NumberCache.TryGetValue(number, out object? numberObject))
            {
                return numberObject;
            }
            else
            {
                numberObject = number;
                if (NumberCache.Count < NumberCacheMaxCount)
                {
                    NumberCache.TryAdd(number, numberObject);
                }
                return numberObject;
            }
        }

        static string GetOrCacheString(Utf8JsonReader reader)
        {
            var stringCacheMaxCharsLength = StringCacheMaxCharsLength;
            if (reader.ValueSpan.Length > stringCacheMaxCharsLength)
            {
                return reader.GetString()!;
            }
            else
            {
                Span<char> stackallocBuffer = stackalloc char[stringCacheMaxCharsLength];
                var len = reader.CopyString(stackallocBuffer);
                if (_stringCacheAlternateLookup.TryGetValue(stackallocBuffer[..len], out var str))
                {
                    return str;
                }
                else
                {
                    str = new string(stackallocBuffer[..len]);
                    if (StringCache.Count < StringCacheMaxCount)
                    {
                        StringCache.TryAdd(str, str);
                    }
                    return str;
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value.GetType() == typeof(string))
        {
            writer.WriteStringValue(value.ToString());
        }
        else if (value.GetType() == typeof(bool))
        {
            writer.WriteBooleanValue((bool)value);
        }
        else if (value.GetType() == typeof(double))
        {
            writer.WriteNumberValue((double)value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
