namespace DstServerQuery.Web.Services.TrafficRateLimiter;

public record struct TrafficChunk
{
    public DateTimeOffset DateTime { get; set; }
    public int Bytes { get; set; }
}
