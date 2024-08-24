using DstServerQuery.Models.Lobby.Interfaces.V2;

namespace DstServerQuery.Web.Models.Http;

/// <summary>
/// 服务器的详细信息
/// </summary>
public class ServerDetailsResponse : ResponseBase
{
    public required ILobbyServerDetailedV2 Server { get; set; }
    public required DateTimeOffset LastUpdate { get; set; }
}
