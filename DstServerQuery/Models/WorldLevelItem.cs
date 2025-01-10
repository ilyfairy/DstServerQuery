using DstServerQuery.Converters;
using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models;

public class WorldLevelRawItem
{
    [JsonPropertyName("__addr")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    public LobbyNumberId Id { get; set; }

    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(LobbySteamIdConverter))]
    public LobbySteamId? SteamId { get; set; } // 有前缀
}

public class WorldLevelItem
{
    public string? Address { get; set; }

    public int Port { get; set; }

    public LobbyNumberId Id { get; set; }

    [JsonConverter(typeof(LobbySteamIdConverter))]
    public LobbySteamId? SteamId { get; set; } // 有前缀

    public static WorldLevelItem FromRaw(WorldLevelRawItem raw)
    {
        WorldLevelItem item = new();
        item.Address = raw.Address;
        item.Port = raw.Port;
        item.Id = raw.Id;
        item.SteamId = raw.SteamId;
        return item;
    }
}