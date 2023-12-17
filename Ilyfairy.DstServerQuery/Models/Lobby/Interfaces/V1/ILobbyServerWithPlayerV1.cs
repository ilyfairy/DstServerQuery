using Ilyfairy.DstServerQuery.Helpers.Converters;
using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

public interface ILobbyServerWithPlayerV1 : ILobbyServerV1
{

    [JsonPropertyName("Players")]
    [JsonConverter(typeof(PlayersInfoWitTranslateConverter))] // NOTE:自定义转换
    public LobbyPlayerInfo[]? Players { get; set; } //玩家信息

}
