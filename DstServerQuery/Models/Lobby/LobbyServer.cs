using DstServerQuery.Converters;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using DstServerQuery.Models.Lobby.Interfaces.V2;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models.Lobby;

/// <summary>
/// 简略信息, 用于反序列化
/// </summary>
public class LobbyServer : ILobbyServerV1, ILobbyServerV2
{
    internal string? _Region;
    internal LobbyPlatform _LobbyPlatform;
    internal DateTimeOffset _LastUpdate;
    internal bool _IsDetailed;

    [JsonPropertyName("name")]
    public string Name { get; set; } //房间名称

    [JsonPropertyName("__addr")]
    [JsonConverter(typeof(IPAddressStringConverter))]
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

    [JsonConverter(typeof(IntentConverter))]
    [JsonPropertyName("intent")]
    public LobbyIntent Intent { get; set; } //风格

    [JsonPropertyName("maxconnections")]
    public int MaxConnections { get; set; } //最大玩家限制

    [JsonConverter(typeof(GameModeConverter))]
    [JsonPropertyName("mode")] // TODO
    public LobbyGameMode Mode { get; set; } //模式

    [JsonPropertyName("mods")]
    public bool IsMods { get; set; } //是否开启mod

    [JsonPropertyName("password")]
    public bool IsPassword { get; set; } //是否需要密码

    //[JsonConverter(typeof(EnumConverter<Platform>))]
    [JsonPropertyName("platform")] // TODO
    public Platform Platform { get; set; } //平台信息

    [JsonConverter(typeof(LobbySeasonConverter))]
    [JsonPropertyName("season")] // TODO
    public LobbySeason Season { get; set; } //季节

    [JsonPropertyName("pvp")]
    public bool IsPvp { get; set; } //是否启用pvp

    [JsonPropertyName("v")]
    public long Version { get; set; } //版本

    [JsonPropertyName("session")]
    public LobbySessionId? Session { get; set; } //会话id

    //V1专用
    [JsonIgnore]
    public string? Country => Address?.IsoCode;




    [JsonPropertyName("clanonly")]
    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    [JsonConverter(typeof(LobbyWorldLevelConverter))]
    [JsonPropertyName("slaves")] // TODO
    public WorldLevelItem[]? Slaves { get; set; } //json

    [JsonConverter(typeof(LobbyWorldLevelConverter))]
    [JsonPropertyName("secondaries")] // TODO
    public WorldLevelItem[]? Secondaries { get; set; } //json

    [JsonPropertyName("allownewplayers")]
    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("serverpaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    //[JsonConverter(typeof(PrefixRemoveConverter))]
    [JsonPropertyName("steamid")]
    public LobbySteamId? SteamId { get; set; } // 有前缀, 可能非数字

    [JsonPropertyName("steamroom")]
    [JsonConverter(typeof(IdCacheConverter))]
    public string? SteamRoom { get; set; }


    [JsonPropertyName("tags")]
    [JsonConverter(typeof(LobbyTagsConverter))]
    public string[]? Tags { get; set; }

    [JsonPropertyName("clienthosted")]
    public bool IsClientHosted { get; set; } // 是否是客户端主机

    [JsonPropertyName("guid")]
    public LobbyGuid? Guid { get; set; } // GUID (一串数字)

    [JsonPropertyName("ownernetid")]
    public LobbySteamId? OwnerNetId { get; set; } // steamid   有前缀

    [JsonPropertyName("steamclanid")]
    public string? SteamClanId { get; set; } // steam群组gid

    [JsonPropertyName("lanonly")]
    public bool IsLanOnly { get; set; } // 是否仅局域网

    public void UpdateFrom(LobbyServer lobbyServer)
    {
        this.Name = lobbyServer.Name;
        this.Address = lobbyServer.Address;
        this.Port = lobbyServer.Port;
        this.RowId = lobbyServer.RowId;
        this.Connected = lobbyServer.Connected;
        this.IsDedicated = lobbyServer.IsDedicated;
        this.Host = lobbyServer.Host;
        this.Intent = lobbyServer.Intent;
        this.MaxConnections = lobbyServer.MaxConnections;
        this.Mode = lobbyServer.Mode;
        this.IsMods = lobbyServer.IsMods;
        this.IsPassword = lobbyServer.IsPassword;
        this.Platform = lobbyServer.Platform;
        this.Season = lobbyServer.Season;
        this.IsPvp = lobbyServer.IsPvp;
        this.Version = lobbyServer.Version;
        this.Session = lobbyServer.Session;
        this.IsClanOnly = lobbyServer.IsClanOnly;
        this.IsFriendsOnly = lobbyServer.IsFriendsOnly;
        this.Slaves = lobbyServer.Slaves;
        this.Secondaries = lobbyServer.Secondaries;
        this.IsAllowNewPlayers = lobbyServer.IsAllowNewPlayers;
        this.IsServerPaused = lobbyServer.IsServerPaused;
        this.SteamId = lobbyServer.SteamId;
        this.SteamRoom = lobbyServer.SteamRoom;
        this.Tags = lobbyServer.Tags;
        this.IsClientHosted = lobbyServer.IsClientHosted;
        this.Guid = lobbyServer.Guid;
        this.OwnerNetId = lobbyServer.OwnerNetId;
        this.SteamClanId = lobbyServer.SteamClanId;
        this.IsLanOnly = lobbyServer.IsLanOnly;
    }

    public DateTimeOffset GetUpdateTime()
    {
        return _LastUpdate;
    }
}
