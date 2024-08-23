using Ilyfairy.DstServerQuery.Helpers.Converters;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

public interface ILobbyServerDetailedV2 : ILobbyServerV2
{
    [JsonPropertyName("Players")]
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息

    [JsonPropertyName("LastPing")]
    public long? LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("Description")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("Tick")]
    public int? Tick { get; set; } //Tick

    [JsonPropertyName("IsClientModsOff")]
    public bool? IsClientModsOff { get; set; }

    [JsonPropertyName("Nat")]
    public int? Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("IsEvent")]
    public bool? IsEvent { get; set; }

    [JsonPropertyName("IsValveCloudServer")]
    public bool? IsValveCloudServer { get; set; }

    [JsonPropertyName("ValvePopId")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("ValveRoutingInfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("IsKleiOfficial")]
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
