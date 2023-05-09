using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

/// <summary>
/// 单个服务器列表详细信息
/// </summary>
public class LobbyDetailsData : LobbyBriefsDataPlayers
{
    [JsonPropertyName("__lastPing")]
    public long LastPing { get; set; } //上次与大厅通信时间

    [JsonPropertyName("steamclanid")]
    public string SteamClanId { get; set; } //steam群组gid

    //TODO: 未完成
    [JsonPropertyName("slaves")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public object Slaves { get; set; } //json

    //TODO: 未完成
    [JsonPropertyName("secondaries")]
    [JsonConverter(typeof(WorldLevelConverter))]
    public object Secondaries { get; set; } //json

    [JsonPropertyName("clanonly")]
    public bool ClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool Fo { get; set; } //是否仅限好友加入

    [JsonPropertyName("guid")]
    public string Guid { get; set; } //GUID

    [JsonPropertyName("clienthosted")]
    public bool ClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("ownernetid")]
    public string OwnerNetId { get; set; } //steamid

    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagsConverter))] // NOTE:自定义转换
    public string[] Tags { get; set; } //Tags

    [JsonPropertyName("lanonly")]
    public bool LanOnly { get; set; } //是否仅局域网

    [JsonPropertyName("desc")]
    public string Desc { get; set; } //房间描述

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
    public bool ValveCloudServer { get; set; }

    [JsonPropertyName("valvepopid")]
    public string ValvePopId { get; set; }

    [JsonPropertyName("valveroutinginfo")]
    public string ValveRoutingInfo { get; set; }

    [JsonPropertyName("kleiofficial")]
    public bool KleiOfficial { get; set; } //是否是官方服务器

    [JsonPropertyName("serverpaused")]
    public bool ServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("data")]
    [JsonConverter(typeof(LobbyDayInfoConverter))] // NOTE:自定义转换
    public LobbyDayInfo DaysInfo { get; set; } //天数信息

    //TODO: 未完成
    [JsonPropertyName("worldgen")]
    [JsonConverter(typeof(WorldGenConverter))]
    public object WorldGen { get; set; } //世界配置

    [JsonPropertyName("steamid")]
    public string SteamId { get; set; }

    [JsonPropertyName("steamroom")]
    public string SteamRoom { get; set; }

    [JsonPropertyName("Users")]
    public object Users { get; set; } //始终为null

    [JsonPropertyName("mods_info")]
    [JsonConverter(typeof(LobbyModInfoConverter))] // NOTE:自定义转换
    public List<LobbyModInfo> ModsInfo { get; set; } //mod信息


    public new void CopyTo(LobbyDetailsData dest)
    {
        if (dest is null) return;
        dest.Name = this.Name;
        dest.Address = this.Address;
        dest.Port = this.Port;
        dest.RowId = this.RowId;
        dest.Connected = this.Connected;
        dest.Dedicated = this.Dedicated;
        dest.Host = this.Host;
        dest.Intent = this.Intent;
        dest.MaxConnections = this.MaxConnections;
        dest.Mode = this.Mode;
        dest.Mods = this.Mods;
        dest.Password = this.Password;
        dest.Platform = this.Platform;
        dest.Season = this.Season;
        dest.PVP = this.PVP;
        dest.Version = this.Version;
        dest.Session = this.Session;

        dest.Players = this.Players;

        dest.LastPing = this.LastPing;
        dest.SteamClanId = this.SteamClanId;
        dest.Slaves = this.Slaves;
        dest.Secondaries = this.Secondaries;
        dest.ClanOnly = this.ClanOnly;
        dest.Fo = this.Fo;
        dest.Guid = this.Guid;
        dest.ClientHosted = this.ClientHosted;
        dest.OwnerNetId = this.OwnerNetId;
        dest.Tags = this.Tags;
        dest.LanOnly = this.LanOnly;
        dest.Desc = this.Desc;
        dest.Tick = this.Tick;
        dest.ClientModsOff = this.ClientModsOff;
        dest.Nat = this.Nat;
        dest.AllowNewPlayers = this.AllowNewPlayers;
        dest.Event = this.Event;
        dest.ValveCloudServer = this.ValveCloudServer;
        dest.ValvePopId = this.ValvePopId;
        dest.ValveRoutingInfo = this.ValveRoutingInfo;
        dest.KleiOfficial = this.KleiOfficial;
        dest.ServerPaused = this.ServerPaused;
        dest.DaysInfo = this.DaysInfo;
        dest.WorldGen = this.WorldGen;
        dest.SteamId = this.SteamId;
        dest.SteamRoom = this.SteamRoom;
        dest.Users = this.Users;
        dest.ModsInfo = this.ModsInfo;

    }

}
