using DstServerQuery.Helpers;
using DstServerQuery.Helpers.Converters.Cache;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models;

public class WorldLevelRawItem
{
    [JsonPropertyName("__addr")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    [JsonConverter(typeof(IdRawCacheConverter))]
    public string Id { get; set; }

    [JsonPropertyName("steamid")]
    public string? SteamId { get; set; } // 有前缀
}

public class WorldLevelItem
{
    public string? Address { get; set; }

    public int Port { get; set; }

    public string Id { get; set; }

    public string? SteamId { get; set; } // 有前缀

    public static WorldLevelItem FromRaw(WorldLevelRawItem raw)
    {
        WorldLevelItem item = new();
        item.Address = raw.Address;
        item.Port = raw.Port;
        item.Id = raw.Id;
        item.SteamId = DstConverterHelper.RemovePrefixColon(raw.SteamId);
        return item;
    }
}