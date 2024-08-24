using System.Collections.Concurrent;

namespace DstServerQuery.Web.Services.TrafficRateLimiter;

public record TrafficContext
{
    public required string IP { get; set; }
    public required ConcurrentQueue<TrafficChunk> Chunks { get; init; }
}