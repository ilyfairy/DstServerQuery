using Ilyfairy.DstServerQuery.Models.Entities;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class ServerCountHistory
{
    public required ICollection<ServerCountInfo> List { get; set; }
}
