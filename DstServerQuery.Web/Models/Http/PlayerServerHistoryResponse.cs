namespace DstServerQuery.Web.Models.Http;

public class PlayerServerHistoryResponse : ResponseBase
{
    public string[] Servers { get; set; } = [];
}
