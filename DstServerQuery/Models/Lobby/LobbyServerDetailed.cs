using System.Text.Json.Serialization;
using DstServerQuery.Converters;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using DstServerQuery.Models.Lobby.Interfaces.V2;

namespace DstServerQuery.Models.Lobby;

/// <summary>
/// 单个服务器列表详细信息, 用于反序列化
/// </summary>
public class LobbyServerDetailed : LobbyServer, ILobbyServerDetailedV1, ILobbyServerDetailedV2
{
    [JsonPropertyName("players")]
    [JsonConverter(typeof(LobbyPlayersInfoConverter))]
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息

    [JsonPropertyName("__lastPing")]
    public long? LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("desc")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("tick")]
    public int? Tick { get; set; } //Tick

    [JsonPropertyName("clientmodsoff")]
    public bool? IsClientModsOff { get; set; }

    [JsonPropertyName("nat")]
    public int? Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("event")]
    public bool? IsEvent { get; set; }

    [JsonPropertyName("valvecloudserver")]
    public bool? IsValveCloudServer { get; set; }

    [JsonPropertyName("valvepopid")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("valveroutinginfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("kleiofficial")]
    public bool? IsKleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("data")]
    [JsonConverter(typeof(LobbyDaysInfoConverter))]
    public LobbyDaysInfo? DaysInfo { get; set; } //天数信息

    [JsonPropertyName("worldgen")]
    [JsonConverter(typeof(LobbyWorldGenConverter))]
    public object? WorldGen { get; set; } //世界配置

    [JsonPropertyName("Users")]
    public object? Users { get; set; } //始终为null

    [JsonPropertyName("mods_info")]
    [JsonConverter(typeof(LobbyModsInfoConverter))]
    public LobbyModInfo[]? ModsInfo { get; set; } //mod信息

    public void UpdateFrom(LobbyServerDetailed lobbyServerDetailed)
    {
        base.UpdateFrom(lobbyServerDetailed);
        Players = lobbyServerDetailed.Players;
        LastPing = lobbyServerDetailed.LastPing;
        Description = lobbyServerDetailed.Description;
        Tick = lobbyServerDetailed.Tick;
        IsClientModsOff = lobbyServerDetailed.IsClientModsOff;
        Nat = lobbyServerDetailed.Nat;
        IsEvent = lobbyServerDetailed.IsEvent;
        IsValveCloudServer = lobbyServerDetailed.IsValveCloudServer;
        ValvePopId = lobbyServerDetailed.ValvePopId;
        ValveRoutingInfo = lobbyServerDetailed.ValveRoutingInfo;
        IsKleiOfficial = lobbyServerDetailed.IsKleiOfficial;
        DaysInfo = lobbyServerDetailed.DaysInfo;
        WorldGen = lobbyServerDetailed.WorldGen;
        Users = lobbyServerDetailed.Users;
        ModsInfo = lobbyServerDetailed.ModsInfo;
    }

}
