using DstServerQuery.Helpers;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using DstServerQuery.Models.Lobby.Interfaces.V2;
using DstServerQuery.Models.Lobby.Units;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models.Lobby;

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


    private ReadOnlyMemory<char>[]? tags;
    public ReadOnlyMemory<char>[]? Tags
    {
        get
        {
            return tags;
        }
        set
        {
            tagsArray = null;
            tags = value;
        }
    }

    private string[]? tagsArray;
    [JsonIgnore]
    public string[]? TagsArray
    {
        get
        {
            if (tags is null) return null;

            var tempTags = new string[tags.Length];
            int i = 0;
            foreach (var item in tags)
            {
                tempTags[i] = item.ToString();
                i++;
            }
            tagsArray = tempTags;
            return tagsArray;
        }
        set
        {
            tags = null;
            tagsArray = value;
        }
    }

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
        if (Address?.IP != Raw.Address)
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
        if (Slaves?.Length != Raw.Slaves?.Count)
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
        Tags = DstConverterHelper.ParseTagsAsMemory(Raw.Tags);
        IsClientHosted = Raw.IsClientHosted;
        Guid = Raw.Guid;
        OwnerNetId = Raw.OwnerNetId;
        IsLanOnly = Raw.IsLanOnly;
        SteamClanId = Raw.SteamClanId;
    }

    private void Merge(LobbyServerRaw otherRaw)
    {
        if (Raw is null)
        {
            Raw = new();
        }

        Raw._LastUpdate = otherRaw._LastUpdate;
        Raw._Region = otherRaw._Region;
        Raw._LobbyPlatform = otherRaw._LobbyPlatform;
        Raw._IsDetailed = otherRaw._IsDetailed;

        Raw.Name = otherRaw.Name;
        Raw.Address = otherRaw.Address;
        Raw.Port = otherRaw.Port;
        Raw.RowId = otherRaw.RowId;
        Raw.Connected = otherRaw.Connected;
        Raw.IsDedicated = otherRaw.IsDedicated;
        Raw.Host = otherRaw.Host;
        Raw.Intent = otherRaw.Intent;
        Raw.MaxConnections = otherRaw.MaxConnections;
        Raw.Mode = otherRaw.Mode;
        Raw.IsMods = otherRaw.IsMods;
        Raw.IsPassword = otherRaw.IsPassword;
        Raw.Platform = otherRaw.Platform;
        Raw.Season = otherRaw.Season;
        Raw.IsPvp = otherRaw.IsPvp;
        Raw.Version = otherRaw.Version;
        Raw.Session = otherRaw.Session;

        Raw.IsClanOnly = otherRaw.IsClanOnly;
        Raw.IsFriendsOnly = otherRaw.IsFriendsOnly;
        Raw.Slaves = otherRaw.Slaves;
        Raw.Secondaries = otherRaw.Secondaries;
        Raw.IsAllowNewPlayers = otherRaw.IsAllowNewPlayers;
        Raw.IsServerPaused = otherRaw.IsServerPaused;
        Raw.SteamId = otherRaw.SteamId;
        Raw.SteamRoom = otherRaw.SteamRoom;
        Raw.Tags = otherRaw.Tags;
        Raw.IsClientHosted = otherRaw.IsClientHosted;
        Raw.Guid = otherRaw.Guid;
        Raw.OwnerNetId = otherRaw.OwnerNetId;
        Raw.IsLanOnly = otherRaw.IsLanOnly;
        Raw.SteamClanId = otherRaw.SteamClanId;
    }


    object ICloneable.Clone() => Clone();

    public virtual LobbyServer Clone()
    {
        LobbyServer obj = new();

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

        obj.Raw = Raw;

        return obj;
    }

    public void CopyTo(LobbyServer dest)
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

        //dest._LastUpdate = this._LastUpdate;
        dest.Raw = Raw;
    }

}
