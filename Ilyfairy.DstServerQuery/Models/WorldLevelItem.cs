using Ilyfairy.DstServerQuery.Helpers.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;

public class LobbyWorldLevel : Dictionary<string, WorldLevelItem>;

public class WorldLevelRawItem
{
    [JsonPropertyName("__addr")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("steamid")]
    public string? SteamId { get; set; } // 有前缀
}

public class WorldLevelItem : IWorldLevelItem
{
    public string? Address { get; set; }

    public int Port { get; set; }

    public string Id { get; set; }

    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀
}

public interface IWorldLevelItem
{
    public string? Address { get; set; }

    public int Port { get; set; }

    public string Id { get; set; }

    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀
}

