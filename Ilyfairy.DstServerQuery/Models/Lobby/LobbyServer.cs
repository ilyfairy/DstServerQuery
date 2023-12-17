using Ilyfairy.DstServerQuery.Helpers.Converters;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

/// <summary>
/// 简略信息, 用于反序列化
/// </summary>
public class LobbyServer : ICloneable, ILobbyServerV1, ILobbyServerV2
{
    internal bool _IsDetails;
    internal string? _Region;
    internal LobbyPlatform _LobbyPlatform;
    internal DateTimeOffset _LastUpdate;
    internal SemaphoreSlim? _Lock;

    public virtual LobbyServerRaw Raw { get; set; }

    public DateTimeOffset GetUpdateTime() => _LastUpdate;

    
    [JsonPropertyName("name")]
    public string Name { get; set; } //房间名称

    [JsonPropertyName("__addr")]
    //[JsonConverter(typeof(IPAddressInfoConverter))]
    public IPAddressInfo Address { get; set; } //ip地址信息

    [JsonPropertyName("port")]
    public int Port { get; set; } //端口

    [JsonPropertyName("__rowId")]
    public string RowId { get; set; } //房间id

    [JsonPropertyName("connected")]
    public int Connected { get; set; } //在线玩家个数

    [JsonPropertyName("dedicated")]
    public bool IsDedicated { get; set; } //是否是专用服务器

    [JsonPropertyName("host")]
    public string Host { get; set; } //房主KleiID

    [JsonPropertyName("intent")]
    [JsonConverter(typeof(IntentConverter))]
    public IntentionType Intent { get; set; } //风格

    [JsonPropertyName("maxconnections")]
    public int MaxConnections { get; set; } //最大玩家限制

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(ModeConverter))]
    public GameMode Mode { get; set; } //模式

    [JsonPropertyName("mods")]
    public bool IsMods { get; set; } //是否开启mod

    [JsonPropertyName("password")]
    public bool IsPassword { get; set; } //是否需要密码

    [JsonPropertyName("platform")]
    //[JsonConverter(typeof(EnumConverter<Platform>))]
    public Platform Platform { get; set; } //平台信息

    [JsonPropertyName("season")]
    [JsonConverter(typeof(SeasonConverter))]
    public Season Season { get; set; } //季节

    [JsonPropertyName("pvp")]
    public bool IsPvp { get; set; } //是否启用pvp

    [JsonPropertyName("v")]
    public long Version { get; set; } //版本

    [JsonPropertyName("session")]
    public string Session { get; set; } //会话id

    //V1专用
    [JsonIgnore]
    public string? Country => Address?.IsoCode;





    [JsonPropertyName("clanonly")]
    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    [JsonPropertyName("slaves")]
    [JsonConverter(typeof(WorldLevelRawConverter))]
    public LobbyWorldLevel? Slaves { get; set; } //json

    [JsonPropertyName("secondaries")]
    [JsonConverter(typeof(WorldLevelRawConverter))]
    public LobbyWorldLevel? Secondaries { get; set; } //json

    [JsonPropertyName("allownewplayers")]
    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("serverpaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀

    [JsonPropertyName("steamroom")]
    public string? SteamRoom { get; set; }

    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagsRawConverter))] // NOTE:自定义转换
    public string[]? Tags { get; set; } //Tags

    [JsonPropertyName("clienthosted")]
    public bool IsClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("ownernetid")]
    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? OwnerNetId { get; set; } //steamid   有前缀

    [JsonPropertyName("steamclanid")]
    public string? SteamClanId { get; set; } //steam群组gid

    [JsonPropertyName("lanonly")]
    public bool IsLanOnly { get; set; } //是否仅局域网










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

        dest._LastUpdate = this._LastUpdate;
    }

}
