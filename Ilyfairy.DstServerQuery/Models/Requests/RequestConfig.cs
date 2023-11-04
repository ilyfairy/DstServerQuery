namespace Ilyfairy.DstServerQuery.Models.Requests;

public class RequestConfig
{
    public string Token { get; set; }
    public string[]? DstDetailsProxyUrls { get; set; }
    public string LobbyProxyTemplate { get; set; } = "https://lobby-v2-cdn.klei.com/{region}-{platform}.json.gz";
}
