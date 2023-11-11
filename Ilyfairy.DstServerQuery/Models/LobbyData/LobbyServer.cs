using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
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
    internal DateTime _LastUpdate;
    internal SemaphoreSlim? _Lock;

    
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

    object ICloneable.Clone() => Clone();

    public virtual LobbyServer Clone()
    {
        LobbyServer obj = new();

        obj.Name = this.Name;
        obj.Address = this.Address with { };
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
    }

}
