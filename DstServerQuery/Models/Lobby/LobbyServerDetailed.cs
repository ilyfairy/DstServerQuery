using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using DstServerQuery.Models.Lobby.Interfaces.V2;

namespace DstServerQuery.Models.Lobby;

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

        obj.Name = Name;
        obj.Address = Address;
        obj.Port = Port;
        obj.RowId = RowId;
        obj.Connected = Connected;
        obj.IsDedicated = IsDedicated;
        obj.Host = Host;
        obj.Intent = Intent;
        obj.MaxConnections = MaxConnections;
        obj.Mode = Mode;
        obj.IsMods = IsMods;
        obj.IsPassword = IsPassword;
        obj.Platform = Platform;
        obj.Season = Season;
        obj.IsPvp = IsPvp;
        obj.Version = Version;
        obj.Session = Session;

        obj.IsClanOnly = IsClanOnly;
        obj.IsFriendsOnly = IsFriendsOnly;
        obj.Slaves = Slaves;
        obj.Secondaries = Secondaries;
        obj.IsAllowNewPlayers = IsAllowNewPlayers;
        obj.IsServerPaused = IsServerPaused;
        obj.SteamId = SteamId;
        obj.SteamRoom = SteamRoom;
        obj.Tags = Tags;
        obj.IsClientHosted = IsClientHosted;
        obj.Guid = Guid;
        obj.OwnerNetId = OwnerNetId;
        obj.IsLanOnly = IsLanOnly;
        obj.SteamClanId = SteamClanId;

        obj.Players = Players?.ToArray();
        obj.LastPing = LastPing;
        obj.Description = Description;
        obj.Tick = Tick;
        obj.IsClientModsOff = IsClientModsOff;
        obj.Nat = Nat;
        obj.IsEvent = IsEvent;
        obj.IsValveCloudServer = IsValveCloudServer;
        obj.ValvePopId = ValvePopId;
        obj.ValveRoutingInfo = ValveRoutingInfo;
        obj.IsKleiOfficial = IsKleiOfficial;
        obj.DaysInfo = DaysInfo is null ? null : DaysInfo with { };
        obj.WorldGen = WorldGen;
        obj.Users = Users;
        obj.ModsInfo = ModsInfo?.ToArray();

        //obj._IsDetails = this._IsDetails;
        //obj._LastUpdate = this._LastUpdate;
        //obj._LobbyPlatform = this._LobbyPlatform;
        //obj._Region = this._Region;

        obj.Raw = Raw;

        return obj;
    }

    public void CopyTo(LobbyServerDetailed dest)
    {
        if (dest is null) return;
        dest.Name = Name;
        dest.Address = Address;
        dest.Port = Port;
        dest.RowId = RowId;
        dest.Connected = Connected;
        dest.IsDedicated = IsDedicated;
        dest.Host = Host;
        dest.Intent = Intent;
        dest.MaxConnections = MaxConnections;
        dest.Mode = Mode;
        dest.IsMods = IsMods;
        dest.IsPassword = IsPassword;
        dest.Platform = Platform;
        dest.Season = Season;
        dest.IsPvp = IsPvp;
        dest.Version = Version;
        dest.Session = Session;

        dest.IsClanOnly = IsClanOnly;
        dest.IsFriendsOnly = IsFriendsOnly;
        dest.Slaves = Slaves;
        dest.Secondaries = Secondaries;
        dest.IsAllowNewPlayers = IsAllowNewPlayers;
        dest.IsServerPaused = IsServerPaused;
        dest.SteamId = SteamId;
        dest.SteamRoom = SteamRoom;
        dest.Tags = Tags;
        dest.IsClientHosted = IsClientHosted;
        dest.Guid = Guid;
        dest.OwnerNetId = OwnerNetId;
        dest.IsLanOnly = IsLanOnly;
        dest.SteamClanId = SteamClanId;

        dest.Players = Players;
        dest.LastPing = LastPing;
        dest.Description = Description;
        dest.Tick = Tick;
        dest.IsClientModsOff = IsClientModsOff;
        dest.Nat = Nat;
        dest.IsEvent = IsEvent;
        dest.IsValveCloudServer = IsValveCloudServer;
        dest.ValvePopId = ValvePopId;
        dest.ValveRoutingInfo = ValveRoutingInfo;
        dest.IsKleiOfficial = IsKleiOfficial;
        dest.DaysInfo = DaysInfo;
        dest.WorldGen = WorldGen;
        dest.Users = Users;
        dest.ModsInfo = ModsInfo;

        //dest._LastUpdate = this._LastUpdate;

        dest.Raw = Raw;
    }

}
