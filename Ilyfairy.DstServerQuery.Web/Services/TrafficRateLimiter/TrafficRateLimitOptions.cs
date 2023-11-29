using System.Collections.Concurrent;

namespace Ilyfairy.DstServerQuery.Web.Services.TrafficRateLimiter;

public class TrafficRateLimitOptions
{
    public ConcurrentDictionary<string, ConcurrentQueue<TrafficChunk>> Users { get; } = new();

    public int StatusCode { get; set; } = 429;
    public int MaxQueue { get; set; } = 1000;

    public string[] IPHeader { get; set; } = [];
    public TrafficRateLimit[] TrafficAny { get; set; } = []; // 任何IP的流量限制器
    public Dictionary<string, TrafficRateLimit[]> TrafficTargets { get; set; } = []; // 特定IP速率限制器

}
