using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DstServerQuery.Helpers;
using DstServerQuery.Models;

namespace DstServerQuery.Converters;

/*
 * [
 *  "workshop-xxxx", // Id
 *  "Name", // Name
 *  1.1.0, // NewVersion
 *  1.0.0, // CurrentVersion
 *  false // IsClientDownload
 * ]
 */

public partial class LobbyModsInfoConverter : JsonConverter<LobbyModInfo[]>
{
    public static ConcurrentStringCacheDictionary ModsVersionCache { get; } = new();
    public static ConcurrentStringCacheDictionary ModsNameCache { get; } = new();

    public override LobbyModInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        List<LobbyModInfo?> list = new();
        LobbyModInfo current = null!;
        int i = 0;
        if (reader.TokenType is not JsonTokenType.StartArray)
        {
            return null;
        }

        Span<char> buffer = stackalloc char[256]; // Name,NewVersion,CurrentVersion buffer

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (i % 5 == 0)
                {
                    current = new();
                    list.Add(current);
                }

                if (i % 5 == 0)
                {
                    using var workshopString = TempUtf8JsonString.From(reader);
                    const string WorkshopPrefix = "workshop-";
                    if (workshopString.String.Span.StartsWith(WorkshopPrefix))
                    {
                        var idString = workshopString.String.Span[WorkshopPrefix.Length..];
                        if (long.TryParse(idString, out var id))
                        {
                            current.Id = id;
                        }
                        else
                        {
                            current.Id = 0;
                        }
                    }
                    else
                    {
                        current.Id = 0;
                    }
                }
                else if (i % 5 == 1)
                {
                    Debug.Assert(reader.TokenType == JsonTokenType.String);
                    if (reader.TokenType is JsonTokenType.String)
                    {
                        Debug.Assert(reader.ValueSpan.Length <= buffer.Length);
                        var len = reader.CopyString(buffer);
                        current.Name = ModsNameCache.GetOrAdd(buffer[..len]);
                    }
                }
                else if (i % 5 == 2)
                {
                    if (reader.TokenType is JsonTokenType.Null)
                    {
                        current.NewVersion = null;
                    }
                    else if (reader.TokenType is JsonTokenType.String)
                    {
                        Debug.Assert(reader.ValueSpan.Length <= 128);
                        var len = reader.CopyString(buffer);
                        var cachedNewVersion = ModsVersionCache.GetOrAdd(buffer[..len]);
                        current.NewVersion = cachedNewVersion;
                    }
                    else
                    {
                        list.RemoveAt(list.Count - 1);
                        i -= i % 5 + 5;
                        continue;
                    }
                }
                else if (i % 5 == 3)
                {
                    if (reader.TokenType is JsonTokenType.Null)
                    {
                        current.CurrentVersion = null;
                    }
                    else if (reader.TokenType is JsonTokenType.String)
                    {
                        Debug.Assert(reader.ValueSpan.Length <= 128);
                        var len = reader.CopyString(buffer);
                        var cachedCurrentVersion = ModsVersionCache.GetOrAdd(buffer[..len]);
                        current.CurrentVersion = cachedCurrentVersion;
                    }
                    else
                    {
                        list.RemoveAt(list.Count - 1);
                        i = i - i % 5 + 5;
                        continue;
                    }
                }
                else if (i % 5 == 4)
                {
                    if (reader.TokenType is not (JsonTokenType.True or JsonTokenType.False))
                    {
                        Debugger.Break();
                    }
                    current.IsClientDownload = reader.GetBoolean();
                }
                i++;
            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
            throw;
        }

        return list.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, LobbyModInfo[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }

}
