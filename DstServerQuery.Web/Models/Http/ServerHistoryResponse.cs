using DstServerQuery.EntityFrameworkCore.Model.Entities;

namespace DstServerQuery.Web.Models.Http;

public class ServerHistoryResponse : ResponseBase
{
    public required DstServerHistory Server { get; set; }
    public required IEnumerable<ServerHistoryItem> Items { get; set; }
}
