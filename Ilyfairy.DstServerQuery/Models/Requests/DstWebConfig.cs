namespace Ilyfairy.DstServerQuery.Models.Requests;

public class DstWebConfig
{
    public string? Token { get; set; }
    public string[]? DstDetailsProxyUrls { get; set; }
    public string LobbyProxyTemplate { get; set; } = "https://lobby-v2-cdn.klei.com/{region}-{platform}.json.gz";

    public long? DstDefaultVersion { get; set; }

    public int? HistoryUpdateInterval { get; set; }
    public int? DetailsUpdateInterval { get; set; }
    public bool? IsDisabledInsertDatabase { get; set; }
}
