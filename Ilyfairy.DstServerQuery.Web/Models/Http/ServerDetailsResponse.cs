using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

/// <summary>
/// 服务器的详细信息
/// </summary>
public class ServerDetailsResponse : ResponseBase
{
    public required ILobbyServerDetailedV2 Server { get; set; }
    public required DateTime LastUpdate { get; set; }
}
