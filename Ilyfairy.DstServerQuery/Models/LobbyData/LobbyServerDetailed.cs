using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

/// <summary>
/// 单个服务器列表详细信息, 用于反序列化
/// </summary>
public class LobbyServerDetailed : LobbyServer, ICloneable, ILobbyServerWithPlayerV1, ILobbyServerDetailedV1, ILobbyServerWithPlayerV2, ILobbyServerDetailedV2
{
    [JsonPropertyName("players")]
    [JsonConverter(typeof(PlayersInfoConverter))] // NOTE:自定义转换
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息


    [JsonPropertyName("__lastPing")]
    public long LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("steamclanid")]
    public string? SteamClanId { get; set; } //steam群组gid

    [JsonPropertyName("slaves")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public Dictionary<string, LobbyWorldLevel>? Slaves { get; set; } //json

    [JsonPropertyName("secondaries")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public Dictionary<string, LobbyWorldLevel>? Secondaries { get; set; } //json

    [JsonPropertyName("clanonly")]
    public bool ClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool IsFo { get; set; } //是否仅限好友加入

    [JsonPropertyName("guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("clienthosted")]
    public bool ClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("ownernetid")]
    public string? OwnerNetId { get; set; } //steamid

    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagsConverter))] // NOTE:自定义转换
    public string[]? Tags { get; set; } //Tags

    [JsonPropertyName("lanonly")]
    public bool LanOnly { get; set; } //是否仅局域网

    [JsonPropertyName("desc")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("tick")]
    public int Tick { get; set; } //Tick

    [JsonPropertyName("clientmodsoff")]
    public bool ClientModsOff { get; set; }

    [JsonPropertyName("nat")]
    public int Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("allownewplayers")]
    public bool AllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("event")]
    public bool Event { get; set; }

    [JsonPropertyName("valvecloudserver")]
    public bool IsValveCloudServer { get; set; }

    [JsonPropertyName("valvepopid")]
    public string? ValvePopId { get; set; }

    [JsonPropertyName("valveroutinginfo")]
    public string? ValveRoutingInfo { get; set; }

    [JsonPropertyName("kleiofficial")]
    public bool IsKleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("serverpaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("data")]
    [JsonConverter(typeof(LobbyDayInfoConverter))] // NOTE:自定义转换
    public LobbyDaysInfo? DaysInfo { get; set; } //天数信息

    //TODO: 未完成
    [JsonPropertyName("worldgen")]
    [JsonConverter(typeof(WorldGenConverter))]
    public object? WorldGen { get; set; } //世界配置

    [JsonPropertyName("steamid")]
    public string? SteamId { get; set; }

    [JsonPropertyName("steamroom")]
    public string? SteamRoom { get; set; }

    [JsonPropertyName("Users")]
    public object? Users { get; set; } //始终为null

    [JsonPropertyName("mods_info")]
    [JsonConverter(typeof(LobbyModInfoConverter))] // NOTE:自定义转换
    public LobbyModInfo[]? ModsInfo { get; set; } //mod信息

    object ICloneable.Clone() => Clone();

    public override LobbyServerDetailed Clone()
    {
        LobbyServerDetailed obj = new();

        obj.Name = this.Name;
        obj.Address = this.Address with { };
        obj.Port = this.Port;
        obj.RowId = this.RowId;
        obj.Connected = this.Connected;
        obj.IsDedicated = this.IsDedicated;
        obj.Host = this.Host;
        obj.Intent = this.Intent;
        obj.MaxConnections = this.MaxConnections;
        obj.Mode = this.Mode;
        obj.IsMods = this.IsMods;
        obj.IsPassword = this.IsPassword;
        obj.Platform = this.Platform;
        obj.Season = this.Season;
        obj.IsPvp = this.IsPvp;
        obj.Version = this.Version;
        obj.Session = this.Session;

        obj.Players = this.Players?.ToArray();

        obj.LastPing = this.LastPing;
        obj.SteamClanId = this.SteamClanId;
        obj.Slaves = this.Slaves;
        obj.Secondaries = this.Secondaries;
        obj.ClanOnly = this.ClanOnly;
        obj.IsFo = this.IsFo;
        obj.Guid = this.Guid;
        obj.ClientHosted = this.ClientHosted;
        obj.OwnerNetId = this.OwnerNetId;
        obj.Tags = this.Tags;
        obj.LanOnly = this.LanOnly;
        obj.Description = this.Description;
        obj.Tick = this.Tick;
        obj.ClientModsOff = this.ClientModsOff;
        obj.Nat = this.Nat;
        obj.AllowNewPlayers = this.AllowNewPlayers;
        obj.Event = this.Event;
        obj.IsValveCloudServer = this.IsValveCloudServer;
        obj.ValvePopId = this.ValvePopId;
        obj.ValveRoutingInfo = this.ValveRoutingInfo;
        obj.IsKleiOfficial = this.IsKleiOfficial;
        obj.IsServerPaused = this.IsServerPaused;
        obj.DaysInfo = this.DaysInfo is null ? null : DaysInfo with { };
        obj.WorldGen = this.WorldGen;
        obj.SteamId = this.SteamId;
        obj.SteamRoom = this.SteamRoom;
        obj.Users = this.Users;
        obj.ModsInfo = this.ModsInfo?.ToArray();

        return obj;
    }

    public void CopyTo(LobbyServerDetailed dest)
    {
        if (dest is null) return;
        dest.Name = this.Name;
        dest.Address = this.Address;
        dest.Port = this.Port;
        dest.RowId = this.RowId;
        dest.Connected = this.Connected;
        dest.IsDedicated = this.IsDedicated;
        dest.Host = this.Host;
        dest.Intent = this.Intent;
        dest.MaxConnections = this.MaxConnections;
        dest.Mode = this.Mode;
        dest.IsMods = this.IsMods;
        dest.IsPassword = this.IsPassword;
        dest.Platform = this.Platform;
        dest.Season = this.Season;
        dest.IsPvp = this.IsPvp;
        dest.Version = this.Version;
        dest.Session = this.Session;

        dest.Players = this.Players;

        dest.LastPing = this.LastPing;
        dest.SteamClanId = this.SteamClanId;
        dest.Slaves = this.Slaves;
        dest.Secondaries = this.Secondaries;
        dest.ClanOnly = this.ClanOnly;
        dest.IsFo = this.IsFo;
        dest.Guid = this.Guid;
        dest.ClientHosted = this.ClientHosted;
        dest.OwnerNetId = this.OwnerNetId;
        dest.Tags = this.Tags;
        dest.LanOnly = this.LanOnly;
        dest.Description = this.Description;
        dest.Tick = this.Tick;
        dest.ClientModsOff = this.ClientModsOff;
        dest.Nat = this.Nat;
        dest.AllowNewPlayers = this.AllowNewPlayers;
        dest.Event = this.Event;
        dest.IsValveCloudServer = this.IsValveCloudServer;
        dest.ValvePopId = this.ValvePopId;
        dest.ValveRoutingInfo = this.ValveRoutingInfo;
        dest.IsKleiOfficial = this.IsKleiOfficial;
        dest.IsServerPaused = this.IsServerPaused;
        dest.DaysInfo = this.DaysInfo;
        dest.WorldGen = this.WorldGen;
        dest.SteamId = this.SteamId;
        dest.SteamRoom = this.SteamRoom;
        dest.Users = this.Users;
        dest.ModsInfo = this.ModsInfo;

    }

}
