namespace Ilyfairy.DstServerQuery.Models.Requests;

public class DstWebConfig
{
    public string? Token { get; set; }
    public string[]? DstDetailsProxyUrls { get; set; }
    public string LobbyProxyTemplate { get; set; } = "https://lobby-v2-cdn.klei.com/{region}-{platform}.json.gz";

    public int? HistoryLiteUpdateInterval { get; set; }
    public int? HistoryDetailsUpdateInterval { get; set; }
    public int? ServerUpdateInterval { get; set; }
    public int? ServerDetailsUpdateInterval { get; set; }

    /// <summary>
    /// 历史记录过期删除时间
    /// </summary>
    public int? HistoryExpiration { get; set; }

    public bool? IsDisabledInsertDatabase { get; set; }

    /// <summary>
    /// HistoryCountService是否从Players属性中计数
    /// </summary>
    public bool IsCountFromPlayers { get; set; }

    /// <summary>
    /// 详细信息更新的线程数
    /// </summary>
    public int? UpdateThreads { get; set; }
}
