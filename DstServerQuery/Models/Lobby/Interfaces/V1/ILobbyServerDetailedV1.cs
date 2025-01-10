using DstServerQuery.Converters;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models.Lobby.Interfaces.V1;

public interface ILobbyServerDetailedV1 : ILobbyServerV1
{
    [JsonPropertyName("Players")]
    [JsonConverter(typeof(PlayersInfoWitTranslateConverter))]
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息

    [JsonPropertyName("LastPing")]
    public long? LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("Desc")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("Tick")]
    public int? Tick { get; set; } //Tick

    [JsonPropertyName("ClientModsOff")]
    public bool? IsClientModsOff { get; set; }

    [JsonPropertyName("Nat")]
    public int? Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("Event")]
    public bool? IsEvent { get; set; }

    [JsonPropertyName("ValveCloudServer")]
    public bool? IsValveCloudServer { get; set; }

    [JsonPropertyName("ValvePopId")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("ValveRoutingInfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("KleiOfficial")]
    public bool? IsKleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("DaysInfo")]
    public LobbyDaysInfo? DaysInfo { get; set; } //天数信息

    [JsonPropertyName("WorldGen")]
    public object? WorldGen { get; set; } //世界配置

    [JsonPropertyName("Users")]
    public object? Users { get; set; } //始终为null

    [JsonPropertyName("ModsInfo")]
    public LobbyModInfo[]? ModsInfo { get; set; } //mod信息
}
