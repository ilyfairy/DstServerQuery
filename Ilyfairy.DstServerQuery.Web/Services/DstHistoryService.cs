using EFCore.BulkExtensions;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.Requests;
using Ilyfairy.DstServerQuery.Web.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ilyfairy.DstServerQuery.Web.Services;

public class DstHistoryService
{
    private readonly ILogger<DstHistoryService> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly HistoryCountManager historyCountManager;

    private DateTime lastServerUpdateDateTime;
    private readonly int historyUpdateInterval = 10 * 60;
    private bool lastIsDetailed = false;

    public DstHistoryService(ILogger<DstHistoryService> logger, DstWebConfig config, IServiceScopeFactory serviceScopeFactory, LobbyServerManager lobbyDetailsManager, HistoryCountManager historyCountManager)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.historyCountManager = historyCountManager;

        if (config.HistoryUpdateInterval is int updateInterval)
        {
            historyUpdateInterval = updateInterval;
        }


        if (config.IsDisabledInsertDatabase is not true)
        {
            lobbyDetailsManager.Updated += LobbyDetailsManager_Updated;
        }
    }

    private async void LobbyDetailsManager_Updated(object sender, DstUpdatedData e)
    {
        if (e.Servers.Count == 0) return;

        DateTime updateDateTime = e.UpdatedDateTime;
        LobbyServerDetailed[] servers = e.Servers.Select(s => s.Clone()).ToArray();

        lock (this)
        {
            if (!lastIsDetailed && e.IsDetailed)
            {
                lastServerUpdateDateTime = DateTime.Now;
            }
            else if (DateTime.Now - lastServerUpdateDateTime > TimeSpan.FromSeconds(historyUpdateInterval))
            {
                lastServerUpdateDateTime = DateTime.Now;
            }
            else
            {
                return;
            }
            lastIsDetailed = e.IsDetailed;
        }

        logger.LogInformation("历史信息储存数据库 IsDetailed:{IsDetailed}", e.IsDetailed);
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();


        try
        {
            await historyCountManager.AddAsync(servers, updateDateTime);

            if (e.IsDetailed)
            {
                await UpdatedDetailed(dbContext, servers, updateDateTime);
            }
            else
            {
                await Updated(dbContext, servers, updateDateTime);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("UpdatedServer失败 IsDetailed:{IsDetailed} Exception:{Exception}", e.IsDetailed, ex.Message);
        }
    }

    private async Task EnsureServersCreated(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers, DateTime createDateTime)
    {
        List<DstServerHistory> list = new();
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
                UpdateTime = createDateTime,
            };
            list.Add(h);
        }

        try
        {
            await dbContext.BulkInsertOrUpdateAsync(list, new BulkConfig()
            {
                SetOutputIdentity = true,
            });
            await dbContext.BulkSaveChangesAsync();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private async Task Updated(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers, DateTime updateDateTime)
    {
        await EnsureServersCreated(dbContext, servers, updateDateTime);

        List<DstServerHistoryItem> updateList = new(servers.Count);
        foreach (var item in servers)
        {
            DstServerHistoryItem hItem = new()
            {
                ServerId = item.RowId,
                DateTime = updateDateTime,
                PlayerCount = item.Connected,
                Season = item.Season.Value,
            };

            updateList.Add(hItem);
        }

        await dbContext.BulkInsertAsync(updateList);
        await dbContext.BulkSaveChangesAsync();
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


    private async Task UpdatedDetailed(DstDbContext dbContext, ICollection<LobbyServerDetailed> servers, DateTime updateDateTime)
    {
        await EnsureServersCreated(dbContext, servers, updateDateTime);

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
                DateTime = updateDateTime,
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
            dbContext.ServerHistoryItems.AddRange(items);
            dbContext.HistoryServerItemPlayerPair.AddRange(pairs);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning("History更新失败 {Exception}", ex.Message);
        }
    }
}

