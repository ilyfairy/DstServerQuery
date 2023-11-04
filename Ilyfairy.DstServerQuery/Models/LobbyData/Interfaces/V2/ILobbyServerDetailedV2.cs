using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

public interface ILobbyServerDetailedV2 : ILobbyServerWithPlayerV2
{
    [JsonPropertyName("LastPing")]
    public long LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("SteamClanId")]
    public string? SteamClanId { get; set; } //steam群组gid

    //TODO: 未完成
    [JsonPropertyName("Slaves")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public object? Slaves { get; set; } //json

    //TODO: 未完成
    [JsonPropertyName("Secondaries")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public object? Secondaries { get; set; } //json

    [JsonPropertyName("ClanOnly")]
    public bool ClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("IsFo")]
    public bool IsFo { get; set; } //是否仅限好友加入

    [JsonPropertyName("Guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("ClientHosted")]
    public bool ClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("OwnerNetId")]
    public string? OwnerNetId { get; set; } //steamid

    [JsonPropertyName("Tags")]
    [JsonConverter(typeof(TagsConverter))] // NOTE:自定义转换
    public string[]? Tags { get; set; } //Tags

    [JsonPropertyName("LanOnly")]
    public bool LanOnly { get; set; } //是否仅局域网

    [JsonPropertyName("Description")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("Tick")]
    public int Tick { get; set; } //Tick

    [JsonPropertyName("ClientModsOff")]
    public bool ClientModsOff { get; set; }

    [JsonPropertyName("Nat")]
    public int Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("AllowNewPlayers")]
    public bool AllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("Event")]
    public bool Event { get; set; }

    [JsonPropertyName("IsValveCloudServer")]
    public bool IsValveCloudServer { get; set; }

    [JsonPropertyName("ValvePopId")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("ValveRoutingInfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("IsKleiOfficial")]
    public bool IsKleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("IsServerPaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("DaysInfo")]
    [JsonConverter(typeof(LobbyDayInfoConverter))] // NOTE:自定义转换
    public LobbyDaysInfo? DaysInfo { get; set; } //天数信息

    //TODO: 未完成
    [JsonPropertyName("WorldGen")]
    [JsonConverter(typeof(WorldGenConverter))]
    public object? WorldGen { get; set; } //世界配置

    [JsonPropertyName("SteamId")]
    public string? SteamId { get; set; }

    [JsonPropertyName("SteamRoom")]
    public string? SteamRoom { get; set; }

    [JsonPropertyName("Users")]
    public object? Users { get; set; } //始终为null

    [JsonPropertyName("ModsInfo")]
    [JsonConverter(typeof(LobbyModInfoConverter))] // NOTE:自定义转换
    public LobbyModInfo[]? ModsInfo { get; set; } //mod信息
}
