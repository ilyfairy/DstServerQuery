using EFCore.BulkExtensions;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.Requests;
using Ilyfairy.DstServerQuery.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Ilyfairy.DstServerQuery.Web.Services;

public class DstHistoryService(ILogger<DstHistoryService> logger, DstWebConfig config, IServiceScopeFactory serviceScopeFactory, LobbyServerManager lobbyServerManager, HistoryCountService historyCountManager) : IHostedService
{
    private readonly CancellationTokenSource cts = new();
    private readonly TimeSpan historyLiteUpdateInterval = TimeSpan.FromSeconds(config.HistoryLiteUpdateInterval ?? 10 * 60);
    private readonly ConcurrentDictionary<string, LobbyServerDetailed> working = new();
    private readonly SemaphoreSlim chunkUpdateLock = new(1);
    private bool serverWorking = false;

    private DateTimeOffset lastHistoryLiteUpdate;


    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (config.IsDisabledInsertDatabase is not true)
        {
            lobbyServerManager.DetailsChunkUpdated += LobbyServerManager_DetailsChunkUpdated;
            lobbyServerManager.ServerUpdated += LobbyServerManager_Updated;
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }

    //详细信息
    private async void LobbyServerManager_DetailsChunkUpdated(object? sender, DstUpdatedDetailsChunk e)
    {
        var chunk = e.Chunk;
        using var scope = serviceScopeFactory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<DstDbContext>();
        db.Database.SetCommandTimeout(TimeSpan.FromSeconds(60));


        try
        {
            await chunkUpdateLock.WaitAsync(cts.Token);
#if DEBUG
            foreach (var item in chunk)
            {
                if (!working.TryAdd(item.RowId, item))
                {
                    var err = e.Chunk.GroupBy(v => v.RowId).Where(v => v.Count() > 1).ToArray();
                }
            }
#endif
            await UpdatedDetailed(db, chunk);
#if DEBUG
            foreach (var item in chunk)
            {
                if (!working.TryRemove(item.RowId, out var server))
                {
                    var a = working.Where(v => v.Key == "KU_1W-yHDWL");
                }
            }
#endif
        }
        catch (Exception ex)
        {
            logger.LogError("UpdateHistoryServerChunk更新异常 {Exception}", ex);
        }
        finally
        {
            chunkUpdateLock.Release();
        }
    }

    //简略信息
    private async void LobbyServerManager_Updated(object? sender, DstUpdatedEventArgs e)
    {
        if (cts.Token.IsCancellationRequested) return;
        if (e.Servers.Count == 0) return;

        DateTimeOffset updateDateTime = e.UpdatedDateTime;

        if (updateDateTime - lastHistoryLiteUpdate <= historyLiteUpdateInterval) // 更新间隔
            return;

        lastHistoryLiteUpdate = updateDateTime;

        logger.LogInformation("历史信息储存数据库");
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();

        ICollection<LobbyServerDetailed> servers = e.Servers.Select(v => v.Clone()).ToArray();

        try
        {
            await historyCountManager.AddAsync(servers, updateDateTime, cts.Token);
            serverWorking = true;
            await UpdatedLite(dbContext, servers);
            serverWorking = false;
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine(ex);
#endif
            logger.LogError("UpdateHistoryServer失败 Exception:{Exception}", ex);
        }
    }



    private async Task EnsureServersCreated(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers)
    {
        List<DstServerHistory> list = new(servers.Count);
        foreach (var server in servers)
        {
            //var server = allMap[unKey];
            var h = new DstServerHistory()
            {
                Id = server.RowId,
                IP = server.Address.IP,
                Port = server.Port,
                Host = server.Host,
                Name = server.Name,
                Platform = server.Platform,
                Intent = server.Intent.Value,
                GameMode = server.Mode.Value,
                UpdateTime = server.GetUpdateTime(),
            };
            list.Add(h);
        }

        try
        {
            await dbContext.BulkInsertOrUpdateAsync(list, new BulkConfig()
            {
                SetOutputIdentity = true,
            }, cancellationToken: cts.Token);
            await dbContext.BulkSaveChangesAsync(cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            logger.LogError("EnsureServersCreated Exception: {Exception}", e);
            throw;
        }
    }
    private async Task EnsurePlayersCreated(DstDbContext dbContext, IEnumerable<KeyValuePair<LobbyServerDetailed, LobbyPlayerInfo>> players)
    {
        HashSet<DstPlayer> updatePlayers = new(10000, DstPlayerEqualityComparer.Instance);

        foreach (var item in players)
        {
            updatePlayers.Add(new DstPlayer()
            {
                Id = item.Value.NetId,
                Name = item.Value.Name,
                Platform = item.Key.Platform,
            });
        }

        try
        {
            await dbContext.BulkInsertOrUpdateAsync(updatePlayers, new BulkConfig()
            {
                SetOutputIdentity = true
            });
            await dbContext.BulkSaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }



    private async Task UpdatedLite(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers)
    {
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        await EnsureServersCreated(dbContext, servers);

        List<DstServerHistoryItem> updateList = new(servers.Count);
        foreach (var item in servers)
        {
            DstServerHistoryItem hItem = new()
            {
                ServerId = item.RowId,
                DateTime = item.GetUpdateTime(),
                PlayerCount = item.Connected,
                Season = item.Season.Value,
            };

            updateList.Add(hItem);
        }

        await dbContext.BulkInsertAsync(updateList, cancellationToken: cts.Token);
        await dbContext.BulkSaveChangesAsync(cancellationToken: cts.Token);
    }
    private async Task UpdatedDetailed(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers)
    {
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        await EnsureServersCreated(dbContext, servers);

        var ServerPlayerKV = servers.SelectMany(s =>
            s.Players?.Select(v => KeyValuePair.Create(s, v)) ?? []
            );
        await EnsurePlayersCreated(dbContext, ServerPlayerKV);

        List<DstServerHistoryItem> items = new();
        List<HistoryServerItemPlayer> pairs = new();
        foreach (var item in servers)
        {
            DstServerHistoryItem hItem = new()
            {
                IsDetailed = true,
                ServerId = item.RowId,
                DateTime = item.GetUpdateTime(),
                PlayerCount = item.Connected,
                Season = item.Season.Value,
                DaysInfo = DstDaysInfo.FromLobby(item.DaysInfo)
            };

            //dbContext.ServerHistoryItems.Add(hItem);
            items.Add(hItem);

            pairs.AddRange(item.Players?.Select(v => new HistoryServerItemPlayer()
            {
                HistoryServerItem = hItem,
                PlayerId = v.NetId
            }) ?? []);
        }

        try
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            await dbContext.ServerHistoryItems.AddRangeAsync(items, cts.Token);
            await dbContext.SaveChangesAsync(cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogError("History Item更新失败 {Exception}", ex.Message);
        }

        foreach (var item in pairs)
        {
            item.HistoryServerItemId = item.HistoryServerItem.Id;
        }

        try
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            await dbContext.HistoryServerItemPlayerPair.AddRangeAsync(pairs, cts.Token);
            await dbContext.SaveChangesAsync(cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogError("History PlayerPair更新失败 {Exception}", ex.Message);
        }

        //var pairsKvs = pairs.Select(v => new
        //{
        //    v.PlayerId,
        //    v.HistoryServerItem.Id,
        //}).ToArray();

        ////有重复的服务器和玩家
        //var uniquePairs = pairs.GroupBy(v => (v.PlayerId, v.HistoryServerItem.Id)).Select(v => v.First());
    }

}

