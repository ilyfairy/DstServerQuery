using DstServerQuery.EntityFrameworkCore.Model;
using DstServerQuery.EntityFrameworkCore.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace DstServerQuery.Web.Services;

public class HistoryCleanupService(IServiceProvider serviceProvider, TimeSpan? expiration) : IHostedService, IDisposable
{
    private readonly ILogger<HistoryCleanupService> _logger = serviceProvider.GetRequiredService<ILogger<HistoryCleanupService>>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private System.Timers.Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (expiration == null)
        {
            return Task.CompletedTask;
        }
        if (expiration.Value < default(TimeSpan))
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("HistoryCleanupService 历史记录清理过期时间: {Expiration}天", expiration.Value.TotalDays);
        _timer = new(TimeSpan.FromHours(1));
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();

        _ = Task.Run(() => Timer_Elapsed(_timer, new(DateTime.Now)));

        return Task.CompletedTask;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs _)
    {
        _logger.LogInformation("HistoryCleanupService 准备清理历史记录");

        using var scope = serviceProvider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<DstDbContext>();

        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

        var cancellationToken = _cancellationTokenSource.Token;

        try
        {
            var e = DateTimeOffset.UtcNow - expiration!.Value;

            while (true)
            {
                await using var transaction = db.Database.BeginTransaction();

                try
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
                        .ToArrayAsync(cancellationToken);

                    if (historyServerItem is null or [])
                    {
                        await transaction.CommitAsync(cancellationToken);
                        _logger.LogInformation("HistoryCleanupService 无需清理");
                        break;
                    }

                    await db.HistoryServerItemPlayerPair
                        .Where(v => historyServerItem.Select(v => v.Id).Contains(v.HistoryServerItemId))
                        .ExecuteDeleteAsync(cancellationToken);

                    var daysInfoIdsToDelete = historyServerItem
                        .Where(v => v.DaysInfoId.HasValue)
                        .Select(v => v.DaysInfoId!.Value)
                        .ToList();

                    if (daysInfoIdsToDelete.Count > 0)
                    {
                        await db.DaysInfos
                            .Where(d => daysInfoIdsToDelete.Contains(d.Id))
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    db.RemoveRange(historyServerItem);

                    await db.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("HistoryCleanupService 清理数量 {Count}", historyServerItem.Length);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HistoryCleanupService 清理异常");
        }

        try
        {
            await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        _timer?.Dispose();
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