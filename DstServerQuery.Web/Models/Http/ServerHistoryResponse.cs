using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class ServerHistoryResponse : ResponseBase
{
    public required DstServerHistory Server { get; set; }
    public required IEnumerable<ServerHistoryItem> Items { get; set; }
}
