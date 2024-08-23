using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class ServerCountHistoryResponse : ResponseBase
{
    public required ICollection<ServerCountInfo> List { get; set; }
}
