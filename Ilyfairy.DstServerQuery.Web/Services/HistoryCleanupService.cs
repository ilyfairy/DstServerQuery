using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Ilyfairy.DstServerQuery.Models;
using Microsoft.EntityFrameworkCore;

namespace Ilyfairy.DstServerQuery.Web.Services;

public class HistoryCleanupService(IServiceProvider serviceProvider, TimeSpan? expiration) : IHostedService
{
    private readonly CancellationTokenSource cts = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (expiration == null)
        {
            return;
        }
        if (expiration.Value < default(TimeSpan))
        {
            return;
        }

        System.Timers.Timer timer = new(TimeSpan.FromMinutes(10));
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs _)
    {
        using var scope = serviceProvider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<DstDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HistoryCleanupService>>();

        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

        try
        {
            var e = DateTimeOffset.UtcNow - expiration!.Value;

            while (true)
            {
                var historyServerItem = await db.ServerHistoryItems
                    .Where(v => v.DateTime < e)
                    .OrderBy(v => v.Id)
                    .Take(10000)
                    .Select(v => new DstServerHistoryItem()
                    {
                        Id = v.Id,
                        DaysInfoId = v.DaysInfoId
                    })
                    .ToArrayAsync(cts.Token);

                if (historyServerItem is null or [])
                    break;

                await db.HistoryServerItemPlayerPair
                    .Where(v => historyServerItem.Select(v => v.Id).Contains(v.HistoryServerItemId))
                    .ExecuteDeleteAsync(cts.Token);

                db.RemoveRange(historyServerItem);

                await db.SaveChangesAsync(cts.Token);
            }
        }
        catch (Exception e)
        {
            logger.LogError("History清理错误 {Exception}", e.Message);
        }

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }
}

public static class HistoryCleanupServiceExtensions
{
    public static IServiceCollection AddHistoryCleanupService(this IServiceCollection serviceDescriptors, int? expirationHours)
    {
        if (expirationHours is null or < 0) return serviceDescriptors;
        serviceDescriptors.AddHostedService(v => new HistoryCleanupService(v, TimeSpan.FromHours(expirationHours.Value)));
        return serviceDescriptors;
    }
}