
using Ilyfairy.DstServerQuery.Helpers.Converters.Cache;
using System.Net;
using System.Text;

namespace Ilyfairy.DstServerQuery.Web.Services;

public class StringCacheService(LobbyServerManager lobbyServerManager) : IHostedService
{
    private readonly CancellationTokenSource cts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        TagsCacheLoop();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }

    public void TagsCacheLoop()
    {
        DateTimeOffset lastUpdate = new();
        lobbyServerManager.ServerUpdated += (sender, e) =>
        {
            if(DateTimeOffset.Now - lastUpdate > TimeSpan.FromHours(24))
            {
                lastUpdate = DateTimeOffset.Now;

                var duplicateTags = e.Servers
                    .Select(v => v.Raw)
                    .Where(v => v != null && v.Tags != null)
                    .Select(v => v!.Tags)
                    .GroupBy(v => v)
                    .Where(v => v.Count() >= 20)
                    .Select(v => v.First());

                foreach (var tag in duplicateTags)
                {
                    TagsRawCacheConverter.Cache[Encoding.UTF8.GetBytes(tag!)] = tag!;
                };
            }
            
        };
    }
}
