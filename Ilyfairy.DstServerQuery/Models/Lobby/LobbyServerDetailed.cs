using Ilyfairy.DstServerQuery.Helpers;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

/// <summary>
/// 单个服务器列表详细信息, 用于反序列化
/// </summary>
public class LobbyServerDetailed : LobbyServer, ICloneable, ILobbyServerDetailedV1, ILobbyServerDetailedV2
{
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息

    public long? LastPing { get; set; } //上次与大厅通信时间

    public string? Description { get; set; } //房间描述

    public int? Tick { get; set; } //Tick

    public bool? IsClientModsOff { get; set; }

    public int? Nat { get; set; } //服务器网络类型  公网5内网7

    public bool? IsEvent { get; set; }

    public bool? IsValveCloudServer { get; set; }

    public string? ValvePopId { get; set; }

    public string? ValveRoutingInfo { get; set; }

    public bool? IsKleiOfficial { get; set; } //是否是官方服务器

    public LobbyDaysInfo? DaysInfo { get; set; } //天数信息

    public object? WorldGen { get; set; } //世界配置

    public object? Users { get; set; } //始终为null

    public LobbyModInfo[]? ModsInfo { get; set; } //mod信息



    public void UpdateFrom(LobbyServerDetailedRaw raw)
    {
        Raw = raw;
        Refresh();
    }

    public new void Refresh()
    {
        base.Refresh();

        //LastPing = Raw.LastPing;
        Description = Raw.Description;
        Tick = Raw.Tick;
        IsClientModsOff = Raw.IsClientModsOff;
        Nat = Raw.Nat;
        IsEvent = Raw.IsEvent;
        IsValveCloudServer = Raw.IsValveCloudServer;
        ValvePopId = Raw.ValvePopId;
        ValveRoutingInfo = Raw.ValveRoutingInfo;
        IsKleiOfficial = Raw.IsKleiOfficial;
        WorldGen = Raw.WorldGen;
        Users = Raw.Users;
        DaysInfo = DstConverterHelper.ParseDays(Raw.DaysInfo);
        ModsInfo = DstConverterHelper.ParseMods(Raw.ModsInfo);
        Players = DstConverterHelper.ParsePlayers(Raw.Players);
    }


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

        obj.IsClanOnly = this.IsClanOnly;
        obj.IsFriendsOnly = this.IsFriendsOnly;
        obj.Slaves = this.Slaves;
        obj.Secondaries = this.Secondaries;
        obj.IsAllowNewPlayers = this.IsAllowNewPlayers;
        obj.IsServerPaused = this.IsServerPaused;
        obj.SteamId = this.SteamId;
        obj.SteamRoom = this.SteamRoom;
        obj.Tags = this.Tags;
        obj.IsClientHosted = this.IsClientHosted;
        obj.Guid = this.Guid;
        obj.OwnerNetId = this.OwnerNetId;
        obj.IsLanOnly = this.IsLanOnly;
        obj.SteamClanId = this.SteamClanId;

        obj.Players = this.Players?.ToArray();
        obj.LastPing = this.LastPing;
        obj.Description = this.Description;
        obj.Tick = this.Tick;
        obj.IsClientModsOff = this.IsClientModsOff;
        obj.Nat = this.Nat;
        obj.IsEvent = this.IsEvent;
        obj.IsValveCloudServer = this.IsValveCloudServer;
        obj.ValvePopId = this.ValvePopId;
        obj.ValveRoutingInfo = this.ValveRoutingInfo;
        obj.IsKleiOfficial = this.IsKleiOfficial;
        obj.DaysInfo = this.DaysInfo is null ? null : DaysInfo with { };
        obj.WorldGen = this.WorldGen;
        obj.Users = this.Users;
        obj.ModsInfo = this.ModsInfo?.ToArray();

        //obj._IsDetails = this._IsDetails;
        //obj._LastUpdate = this._LastUpdate;
        //obj._LobbyPlatform = this._LobbyPlatform;
        //obj._Region = this._Region;

        obj.Raw = this.Raw;

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

        dest.IsClanOnly = this.IsClanOnly;
        dest.IsFriendsOnly = this.IsFriendsOnly;
        dest.Slaves = this.Slaves;
        dest.Secondaries = this.Secondaries;
        dest.IsAllowNewPlayers = this.IsAllowNewPlayers;
        dest.IsServerPaused = this.IsServerPaused;
        dest.SteamId = this.SteamId;
        dest.SteamRoom = this.SteamRoom;
        dest.Tags = this.Tags;
        dest.IsClientHosted = this.IsClientHosted;
        dest.Guid = this.Guid;
        dest.OwnerNetId = this.OwnerNetId;
        dest.IsLanOnly = this.IsLanOnly;
        dest.SteamClanId = this.SteamClanId;

        dest.Players = this.Players;
        dest.LastPing = this.LastPing;
        dest.Description = this.Description;
        dest.Tick = this.Tick;
        dest.IsClientModsOff = this.IsClientModsOff;
        dest.Nat = this.Nat;
        dest.IsEvent = this.IsEvent;
        dest.IsValveCloudServer = this.IsValveCloudServer;
        dest.ValvePopId = this.ValvePopId;
        dest.ValveRoutingInfo = this.ValveRoutingInfo;
        dest.IsKleiOfficial = this.IsKleiOfficial;
        dest.DaysInfo = this.DaysInfo;
        dest.WorldGen = this.WorldGen;
        dest.Users = this.Users;
        dest.ModsInfo = this.ModsInfo;

        //dest._LastUpdate = this._LastUpdate;

        dest.Raw = this.Raw;
    }

}
