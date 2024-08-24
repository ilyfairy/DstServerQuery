using DstServerQuery.EntityFrameworkCore.Model.Entities;

namespace DstServerQuery.Web.Models.Http;

public class ServerCountHistoryResponse : ResponseBase
{
    public required ICollection<ServerCountInfo> List { get; set; }
}
