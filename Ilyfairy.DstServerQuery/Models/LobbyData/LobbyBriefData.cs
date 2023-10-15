using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

public class LobbyBriefsData : IDisposable
{
    internal bool _IsDetails;
    internal string? _Region;
    internal LobbyPlatform _LobbyPlatform;
    internal DateTime _LastUpdate;
    internal SemaphoreSlim? _Lock;

    
    [JsonPropertyName("name")]
    public string Name { get; set; } //房间名称

    [JsonPropertyName("__addr")]
    //[JsonConverter(typeof(IPAddressInfoConverter))]
    public IPAddressInfo Address { get; set; } //ip地址

    [JsonPropertyName("port")]
    public int Port { get; set; } //端口

    [JsonPropertyName("__rowId")]
    public string RowId { get; set; } //房间id

    [JsonPropertyName("connected")]
    public int Connected { get; set; } //在线玩家个数

    [JsonPropertyName("dedicated")]
    public bool Dedicated { get; set; } //是否是专用服务器

    [JsonPropertyName("host")]
    public string Host { get; set; } //房主KleiID

    [JsonPropertyName("intent")]
    [JsonConverter(typeof(EnumConverter<IntentionType>))]
    public IntentionType Intent { get; set; } //风格

    [JsonPropertyName("maxconnections")]
    public int MaxConnections { get; set; } //最大玩家限制

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(EnumConverter<GameMode>))]
    public GameMode Mode { get; set; } //模式

    [JsonPropertyName("mods")]
    public bool Mods { get; set; } //是否开启mod

    [JsonPropertyName("password")]
    public bool Password { get; set; } //是否需要密码

    [JsonPropertyName("platform")]
    [JsonConverter(typeof(EnumConverter<Platform>))]
    public Platform Platform { get; set; } //平台信息

    [JsonPropertyName("season")]
    [JsonConverter(typeof(EnumConverter<Season>))]
    public Season Season { get; set; } //季节

    [JsonPropertyName("pvp")]
    public bool PVP { get; set; } //是否启用pvp

    [JsonPropertyName("v")]
    public int Version { get; set; } //版本

    [JsonPropertyName("session")]
    public string Session { get; set; } //会话id

    public string? Country => Address?.Country?.IsoCode;


    public void CopyTo(LobbyDetailsData dest)
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
    }

    public void Dispose()
    {
        _Lock?.Dispose();
    }

    ~LobbyBriefsData()
    {
        Dispose();
    }
}
