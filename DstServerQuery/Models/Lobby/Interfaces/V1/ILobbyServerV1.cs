using DstServerQuery.Helpers.Converters;
using DstServerQuery.Models.Lobby.Units;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models.Lobby.Interfaces.V1;

public interface ILobbyServerV1
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } //房间名称

    [JsonPropertyName("Address")]
    [JsonConverter(typeof(IPAddressStringConverter))]
    public IPAddressInfo Address { get; set; } //ip地址信息

    [JsonPropertyName("Port")]
    public int Port { get; set; } //端口

    [JsonPropertyName("RowId")]
    public string RowId { get; set; } //房间id

    [JsonPropertyName("Connected")]
    public int Connected { get; set; } //在线玩家个数

    [JsonPropertyName("Dedicated")]
    public bool IsDedicated { get; set; } //是否是专用服务器

    [JsonPropertyName("Host")]
    public string Host { get; set; } //房主KleiID

    [JsonPropertyName("Intent")]
    [JsonConverter(typeof(IntentWithTranslateConverter))]
    public IntentionType Intent { get; set; } //风格

    [JsonPropertyName("MaxConnections")]
    public int MaxConnections { get; set; } //最大玩家限制

    [JsonPropertyName("Mode")]
    [JsonConverter(typeof(GameModeWithTranslateConverter))]
    public GameMode Mode { get; set; } //模式

    [JsonPropertyName("Mods")]
    public bool IsMods { get; set; } //是否开启mod

    [JsonPropertyName("Password")]
    public bool IsPassword { get; set; } //是否需要密码

    [JsonPropertyName("Platform")]
    public Platform Platform { get; set; } //平台信息

    [JsonPropertyName("Season")]
    [JsonConverter(typeof(SeasonWithTranslateConverter))]
    public Season Season { get; set; } //季节

    [JsonPropertyName("PVP")]
    public bool IsPvp { get; set; } //是否启用pvp

    [JsonPropertyName("Version")]
    public long Version { get; set; } //版本

    [JsonPropertyName("Session")]
    public string Session { get; set; } //会话id

    [JsonPropertyName("Country")]
    public string? Country { get; } //



    [JsonPropertyName("ClanOnly")]
    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("Fo")]
    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    [JsonPropertyName("Slaves")]
    public WorldLevelItem[]? Slaves { get; set; } //json

    [JsonPropertyName("Secondaries")]
    public WorldLevelItem[]? Secondaries { get; set; } //json

    [JsonPropertyName("ServerPaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("AllowNewPlayers")]
    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("SteamId")]
    public string? SteamId { get; set; }

    [JsonPropertyName("SteamRoom")]
    public string? SteamRoom { get; set; }

    [JsonPropertyName("Tags")]
    public ReadOnlyMemory<char>[]? Tags { get; set; } //Tags

    [JsonPropertyName("ClientHosted")]
    public bool IsClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("Guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("OwnerNetId")]
    public string? OwnerNetId { get; set; } //steamid

    [JsonPropertyName("SteamClanId")]
    public string? SteamClanId { get; set; } //steam群组gid

    [JsonPropertyName("LanOnly")]
    public bool IsLanOnly { get; set; } //是否仅局域网

}
