using Ilyfairy.DstServerQuery.Helpers;
using Ilyfairy.DstServerQuery.Helpers.Converters;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

/// <summary>
/// 简略信息, 用于反序列化
/// </summary>
public class LobbyServer : ICloneable, ILobbyServerV1, ILobbyServerV2
{
    internal SemaphoreSlim? _Lock;
    public LobbyServerDetailedRaw? Raw { get; set; }

    public DateTimeOffset GetUpdateTime() => Raw?._LastUpdate ?? throw new Exception("Raw不能为null");

    
    public string Name { get; set; } //房间名称

    //[JsonConverter(typeof(IPAddressInfoConverter))]
    public IPAddressInfo Address { get; set; } //ip地址信息

    public int Port { get; set; } //端口

    public string RowId { get; set; } //房间id

    public int Connected { get; set; } //在线玩家个数

    public bool IsDedicated { get; set; } //是否是专用服务器

    public string Host { get; set; } //房主KleiID

    //[JsonConverter(typeof(IntentConverter))]
    public IntentionType Intent { get; set; } //风格

    public int MaxConnections { get; set; } //最大玩家限制

    //[JsonConverter(typeof(ModeConverter))]
    public GameMode Mode { get; set; } //模式

    public bool IsMods { get; set; } //是否开启mod

    public bool IsPassword { get; set; } //是否需要密码

    //[JsonConverter(typeof(EnumConverter<Platform>))]
    public Platform Platform { get; set; } //平台信息

    //[JsonConverter(typeof(SeasonConverter))]
    public Season Season { get; set; } //季节

    public bool IsPvp { get; set; } //是否启用pvp

    public long Version { get; set; } //版本

    public string Session { get; set; } //会话id

    //V1专用
    [JsonIgnore]
    public string? Country => Address?.IsoCode;





    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    //[JsonConverter(typeof(WorldLevelRawConverter))]
    public WorldLevelItem[]? Slaves { get; set; } //json

    //[JsonConverter(typeof(WorldLevelRawConverter))]
    public WorldLevelItem[]? Secondaries { get; set; } //json

    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

    public bool IsServerPaused { get; set; } //世界是否暂停

    //[JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀

    public string? SteamRoom { get; set; }

    public string[]? Tags { get; set; } //Tags

    public bool IsClientHosted { get; set; } //是否是客户端主机

    public string? Guid { get; set; } //GUID

    //[JsonConverter(typeof(PrefixRemoveConverter))]
    public string? OwnerNetId { get; set; } //steamid   有前缀

    public string? SteamClanId { get; set; } //steam群组gid

    public bool IsLanOnly { get; set; } //是否仅局域网






    public void UpdateFrom(LobbyServerRaw raw)
    {
        Merge(raw);
        Refresh();
    }

    [MemberNotNull(nameof(Raw))]
    public void Refresh()
    {
        ArgumentNullException.ThrowIfNull(Raw);

        Name = Raw.Name;
        if(Address?.IP != Raw.Address)
        {
            Address = DstConverterHelper.ParseAddress(Raw.Address);
        }
        Port = Raw.Port;
        RowId = Raw.RowId;
        Connected = Raw.Connected;
        IsDedicated = Raw.IsDedicated;
        Host = Raw.Host;
        Intent = new IntentionType(Raw.Intent);
        MaxConnections = Raw.MaxConnections;
        Mode = new GameMode(Raw.Mode);
        IsMods = Raw.IsMods;
        IsPassword = Raw.IsPassword;
        Platform = (Platform)Raw.Platform;
        Season = new Season(Raw.Season);
        IsPvp = Raw.IsPvp;
        Version = Raw.Version;
        Session = Raw.Session;
        IsClanOnly = Raw.IsClanOnly;
        IsFriendsOnly = Raw.IsFriendsOnly;
        if(Slaves?.Length != Raw.Slaves?.Count)
        {
            Slaves = DstConverterHelper.WorldLevelRawToArray(Raw.Slaves);
        }
        if (Secondaries?.Length != Raw.Secondaries?.Count)
        {
            Secondaries = DstConverterHelper.WorldLevelRawToArray(Raw.Secondaries);
        }
        IsAllowNewPlayers = Raw.IsAllowNewPlayers;
        IsServerPaused = Raw.IsServerPaused;
        SteamId = Raw.SteamId;
        SteamRoom = Raw.SteamRoom;
        Tags = DstConverterHelper.ParseTags(Raw.Tags);
        IsClientHosted = Raw.IsClientHosted;
        Guid = Raw.Guid;
        OwnerNetId = Raw.OwnerNetId;
        IsLanOnly = Raw.IsLanOnly;
        SteamClanId = Raw.SteamClanId;
    }

    private void Merge(LobbyServerRaw otherRaw)
    {
        if (this.Raw is null)
        {
            Raw = new();
        }

        Raw._LastUpdate = otherRaw._LastUpdate;
        Raw._Region = otherRaw._Region;
        Raw._LobbyPlatform = otherRaw._LobbyPlatform;
        Raw._IsDetailed = otherRaw._IsDetailed;

        this.Raw.Name = otherRaw.Name;
        this.Raw.Address = otherRaw.Address;
        this.Raw.Port = otherRaw.Port;
        this.Raw.RowId = otherRaw.RowId;
        this.Raw.Connected = otherRaw.Connected;
        this.Raw.IsDedicated = otherRaw.IsDedicated;
        this.Raw.Host = otherRaw.Host;
        this.Raw.Intent = otherRaw.Intent;
        this.Raw.MaxConnections = otherRaw.MaxConnections;
        this.Raw.Mode = otherRaw.Mode;
        this.Raw.IsMods = otherRaw.IsMods;
        this.Raw.IsPassword = otherRaw.IsPassword;
        this.Raw.Platform = otherRaw.Platform;
        this.Raw.Season = otherRaw.Season;
        this.Raw.IsPvp = otherRaw.IsPvp;
        this.Raw.Version = otherRaw.Version;
        this.Raw.Session = otherRaw.Session;

        this.Raw.IsClanOnly = otherRaw.IsClanOnly;
        this.Raw.IsFriendsOnly = otherRaw.IsFriendsOnly;
        this.Raw.Slaves = otherRaw.Slaves;
        this.Raw.Secondaries = otherRaw.Secondaries;
        this.Raw.IsAllowNewPlayers = otherRaw.IsAllowNewPlayers;
        this.Raw.IsServerPaused = otherRaw.IsServerPaused;
        this.Raw.SteamId = otherRaw.SteamId;
        this.Raw.SteamRoom = otherRaw.SteamRoom;
        this.Raw.Tags = otherRaw.Tags;
        this.Raw.IsClientHosted = otherRaw.IsClientHosted;
        this.Raw.Guid = otherRaw.Guid;
        this.Raw.OwnerNetId = otherRaw.OwnerNetId;
        this.Raw.IsLanOnly = otherRaw.IsLanOnly;
        this.Raw.SteamClanId = otherRaw.SteamClanId;
    }


    object ICloneable.Clone() => Clone();

    public virtual LobbyServer Clone()
    {
        LobbyServer obj = new();

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

        obj.Raw = this.Raw;

        return obj;
    }

    public void CopyTo(LobbyServer dest)
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

        //dest._LastUpdate = this._LastUpdate;
        dest.Raw = this.Raw;
    }

}
