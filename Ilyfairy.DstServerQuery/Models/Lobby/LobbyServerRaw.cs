using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

public class LobbyServerRaw
{
    [JsonPropertyName("name")]
    public string Name { get; set; } //房间名称

    [JsonPropertyName("__addr")]
    public string Address { get; set; } //ip地址信息

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
    public string Intent { get; set; } //风格

    [JsonPropertyName("maxconnections")]
    public int MaxConnections { get; set; } //最大玩家限制

    [JsonPropertyName("mode")]
    public string Mode { get; set; } //模式

    [JsonPropertyName("mods")]
    public bool IsMods { get; set; } //是否开启mod

    [JsonPropertyName("password")]
    public bool IsPassword { get; set; } //是否需要密码

    [JsonPropertyName("platform")]
    public int Platform { get; set; } //平台信息

    [JsonPropertyName("season")]
    public string Season { get; set; } //季节

    [JsonPropertyName("pvp")]
    public bool IsPvp { get; set; } //是否启用pvp

    [JsonPropertyName("v")]
    public long Version { get; set; } //版本

    [JsonPropertyName("session")]
    public string Session { get; set; } //会话id




    [JsonPropertyName("clanonly")]
    public bool IsClanOnly { get; set; } //仅限steam群组成员加入

    [JsonPropertyName("fo")]
    public bool IsFriendsOnly { get; set; } //是否仅限好友加入

    [JsonPropertyName("slaves")]
    public Dictionary<string, WorldLevelRawItem>? Slaves { get; set; } //json

    [JsonPropertyName("secondaries")]
    public Dictionary<string, WorldLevelRawItem>? Secondaries { get; set; } //json

    [JsonPropertyName("allownewplayers")]
    public bool IsAllowNewPlayers { get; set; } //是否允许新玩家加入

    [JsonPropertyName("serverpaused")]
    public bool IsServerPaused { get; set; } //世界是否暂停

    [JsonPropertyName("steamid")]
    public string? SteamId { get; set; } // 有前缀

    [JsonPropertyName("steamroom")]
    public string? SteamRoom { get; set; }

    [JsonPropertyName("tags")]
    public string? Tags { get; set; } //Tags

    [JsonPropertyName("clienthosted")]
    public bool IsClientHosted { get; set; } //是否是客户端主机

    [JsonPropertyName("guid")]
    public string? Guid { get; set; } //GUID

    [JsonPropertyName("ownernetid")]
    public string? OwnerNetId { get; set; } //steamid   有前缀

    [JsonPropertyName("lanonly")]
    public bool IsLanOnly { get; set; } //是否仅局域网

    [JsonPropertyName("steamclanid")]
    public string? SteamClanId { get; set; } //steam群组gid
    
}
