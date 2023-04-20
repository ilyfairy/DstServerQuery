namespace Ilyfairy.DstServerQuery.Models.Requests;

public class RequestRoot
{
    public string[] OldList { get; set; }
    public string Token { get; set; }
    public string? DstDetailsProxyUrl { get; set; }
}
