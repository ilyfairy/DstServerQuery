namespace Ilyfairy.DstServerQuery.Models.Requests;

public class RequestRoot
{
    public string Token { get; set; }
    public string[]? DstDetailsProxyUrls { get; set; }
}
