using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

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
    [JsonConverter(typeof(ModeWithTranslateConverter))]
    public GameMode Mode { get; set; } //模式

    [JsonPropertyName("Mods")]
    public bool IsMods { get; set; } //是否开启mod

    [JsonPropertyName("Password")]
    public bool IsPassword { get; set; } //是否需要密码

    [JsonPropertyName("Platform")]
    //[JsonConverter(typeof(EnumConverter<Platform>))]
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
}
