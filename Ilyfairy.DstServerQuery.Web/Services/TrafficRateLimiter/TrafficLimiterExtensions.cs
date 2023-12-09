using Microsoft.AspNetCore.Http.Features;
using Serilog;
using System.Collections.Concurrent;

namespace Ilyfairy.DstServerQuery.Web.Services.TrafficRateLimiter;

public static class TrafficLimiterExtensions
{
    public static IServiceCollection AddTrafficLimiter(this IServiceCollection serviceDescriptors, Action<TrafficRateLimitOptions>? configureOptions = null)
    {
        TrafficRateLimitOptions trafficRateLimitOptions = new();
        serviceDescriptors.AddSingleton(trafficRateLimitOptions);
        configureOptions?.Invoke(trafficRateLimitOptions);

        return serviceDescriptors;
    }

    

    public static IApplicationBuilder UseTrafficLimiter(this IApplicationBuilder applicationBuilder)
    {
        TrafficRateLimitOptions trafficRateLimitOptions = applicationBuilder.ApplicationServices.GetRequiredService<TrafficRateLimitOptions>();

        UseTrafficLimiter(applicationBuilder, (context, trafficContext, next) =>
        {
            return Task.CompletedTask;
        });

        return applicationBuilder;
    }

    public static IApplicationBuilder UseTrafficLimiter(this IApplicationBuilder applicationBuilder, Func<HttpContext, TrafficContext, RequestDelegate, Task> limitedCallback)
    {
        TrafficRateLimitOptions trafficRateLimitOptions = applicationBuilder.ApplicationServices.GetRequiredService<TrafficRateLimitOptions>();

        applicationBuilder.Use(async (HttpContext httpContext, RequestDelegate next) =>
        {
            IHttpResponseBodyFeature? originStream = httpContext.Features.Get<IHttpResponseBodyFeature>()!;
            TrafficMonitorStream trafficMonitorStream = new(originStream);
            httpContext.Features.Set<IHttpResponseBodyFeature>(trafficMonitorStream);

            string? ip = null;
            foreach (var key in trafficRateLimitOptions.IPHeader)
            {
                if (httpContext.Request.Headers.TryGetValue(key, out var value))
                {
                    ip = value;
                    break;
                }
            }
            ip ??= httpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            TrafficRateLimit[]? limits = null;
            if (trafficRateLimitOptions.TrafficTargets?.TryGetValue(ip, out limits) != true)
            {
                limits = trafficRateLimitOptions.TrafficAny;
            }
            limits ??= [];

            if (!trafficRateLimitOptions.Users.TryGetValue(ip, out var chunks))
            {
                chunks = new ConcurrentQueue<TrafficChunk>();
                trafficRateLimitOptions.Users[ip] = chunks;
            }

            foreach (var limit in limits)
            {
                DateTimeOffset start = DateTimeOffset.Now - TimeSpan.FromSeconds(limit.WindowSec);
                var range = chunks.Where(v => v.DateTime >= start);
                var sum = range.Sum(v => v.Bytes);
                TrafficContext trafficContext = new()
                {
                    Chunks = chunks,
                    IP = ip
                };

                if (sum > limit.TrafficBytes)
                {
                    //请求被限制
                    httpContext.Response.StatusCode = trafficRateLimitOptions.StatusCode;
                    httpContext.Response.Headers.RetryAfter = limit.WindowSec.ToString();
                    await limitedCallback(httpContext, trafficContext, next);
                    httpContext.Features.Set(originStream);
                    return;
                }
            }

            await next(httpContext);
            await trafficMonitorStream.FlushAsync();

            TrafficChunk currentChunk = new()
            {
                Bytes = trafficMonitorStream.Bytes,
                DateTime = DateTimeOffset.Now,
            };
            chunks.Enqueue(currentChunk);

            while (chunks.Count > trafficRateLimitOptions.MaxQueue)
            {
                chunks.TryDequeue(out var chunk);
            }

            //sum += currentChunk.Bytes;
            //bool isLimit = false;
            //if (sum > limit.TrafficBytes)
            //{
            //    await limitedCallback(httpContext, trafficContext, next);
            //}

            //TrafficContext trafficContext = new()
            //{
            //    Chunks = chunks,
            //    Current = currentChunk,
            //    IsLimit = isLimit,
            //};

            httpContext.Features.Set(originStream);
        });

        return applicationBuilder;
    }
}
