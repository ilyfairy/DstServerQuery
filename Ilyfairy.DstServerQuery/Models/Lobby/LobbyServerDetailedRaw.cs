using Ilyfairy.DstServerQuery.Helpers.Converters.Cache;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

public class LobbyServerDetailedRaw : LobbyServerRaw
{
    [JsonPropertyName("players")]
    [JsonConverter(typeof(PlayersRawCacheConverter))]
    public string? Players { get; set; } //玩家信息

    //[JsonPropertyName("__lastPing")]
    //public long LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("desc")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("tick")]
    public int Tick { get; set; } //Tick

    [JsonPropertyName("clientmodsoff")]
    public bool IsClientModsOff { get; set; }

    [JsonPropertyName("nat")]
    public int Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("event")]
    public bool IsEvent { get; set; }

    [JsonPropertyName("valvecloudserver")]
    public bool IsValveCloudServer { get; set; }

    [JsonPropertyName("valvepopid")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("valveroutinginfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("kleiofficial")]
    public bool IsKleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("data")]
    [JsonConverter(typeof(DaysRawCacheConverter))]
    public string? DaysInfo { get; set; } //天数信息

    [JsonPropertyName("worldgen")]
    [JsonConverter(typeof(WorldGenRawCacheConverter))]
    public string? WorldGen { get; set; } //世界配置


    [JsonPropertyName("Users")]
    public object? Users { get; set; } //始终为null

    [JsonPropertyName("mods_info")]
    [JsonConverter(typeof(ModsInfoRawCacheConverter))]
    public object[]? ModsInfo { get; set; } // bool or string
}
