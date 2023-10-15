using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData;

public class LobbyBriefsDataPlayers : LobbyBriefsData
{
    [JsonPropertyName("players")]
    [JsonConverter(typeof(PlayersInfoConverter))] // NOTE:自定义转换
    [JsonPropertyOrder(-99)]
    public List<LobbyPlayerInfo>? Players { get; set; } //玩家信息


    public new void CopyTo(LobbyDetailsData dest)
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

        dest.Players = this.Players;
    }

}