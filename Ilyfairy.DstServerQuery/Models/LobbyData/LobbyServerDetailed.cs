using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
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
    [JsonConverter(typeof(WorldLevelRawConverter))]
    public LobbyWorldLevel? Slaves { get; set; } //json

    [JsonPropertyName("secondaries")]
    [JsonConverter(typeof(WorldLevelRawConverter))]
    public LobbyWorldLevel? Secondaries { get; set; } //json

    [JsonPropertyName("clanonly")]
    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    [JsonPropertyName("guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("clienthosted")]
    public bool IsClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("ownernetid")]
    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? OwnerNetId { get; set; } //steamid   有前缀

    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagsRawConverter))] // NOTE:自定义转换
    public string[]? Tags { get; set; } //Tags

    [JsonPropertyName("lanonly")]
    public bool IsLanOnly { get; set; } //是否仅局域网

    [JsonPropertyName("desc")]
    public string? Description { get; set; } //房间描述

    [JsonPropertyName("tick")]
    public int Tick { get; set; } //Tick

    [JsonPropertyName("clientmodsoff")]
    public bool IsClientModsOff { get; set; }

    [JsonPropertyName("nat")]
    public int Nat { get; set; } //服务器网络类型  公网5内网7

    [JsonPropertyName("allownewplayers")]
    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

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
    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀

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
        obj.Address = this.Address;
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
        obj.IsClanOnly = this.IsClanOnly;
        obj.IsFriendsOnly = this.IsFriendsOnly;
        obj.Guid = this.Guid;
        obj.IsClientHosted = this.IsClientHosted;
        obj.OwnerNetId = this.OwnerNetId;
        obj.Tags = this.Tags;
        obj.IsLanOnly = this.IsLanOnly;
        obj.Description = this.Description;
        obj.Tick = this.Tick;
        obj.IsClientModsOff = this.IsClientModsOff;
        obj.Nat = this.Nat;
        obj.IsAllowNewPlayers = this.IsAllowNewPlayers;
        obj.IsEvent = this.IsEvent;
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

        obj._IsDetails = this._IsDetails;
        obj._LastUpdate = this._LastUpdate;
        obj._LobbyPlatform = this._LobbyPlatform;
        obj._Region = this._Region;

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
        dest.IsClanOnly = this.IsClanOnly;
        dest.IsFriendsOnly = this.IsFriendsOnly;
        dest.Guid = this.Guid;
        dest.IsClientHosted = this.IsClientHosted;
        dest.OwnerNetId = this.OwnerNetId;
        dest.Tags = this.Tags;
        dest.IsLanOnly = this.IsLanOnly;
        dest.Description = this.Description;
        dest.Tick = this.Tick;
        dest.IsClientModsOff = this.IsClientModsOff;
        dest.Nat = this.Nat;
        dest.IsAllowNewPlayers = this.IsAllowNewPlayers;
        dest.IsEvent = this.IsEvent;
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

        dest._LastUpdate = this._LastUpdate;
    }

}
