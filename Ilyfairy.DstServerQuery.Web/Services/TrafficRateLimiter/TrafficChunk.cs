namespace Ilyfairy.DstServerQuery.Web.Services.TrafficRateLimiter;

public record struct TrafficChunk
{
    public DateTime DateTime { get; set; }
    public int Bytes { get; set; }
}
