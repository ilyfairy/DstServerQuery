using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore;

/// <summary>
/// 大厅服务器历史房间数量管理器
/// </summary>
public class HistoryCountService
{
    private readonly ILogger<HistoryCountService> _logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly Queue<ServerCountInfo> cache = new(10100);

    public DateTimeOffset LastUpdate { get; private set; }
    public DateTimeOffset First => cache.FirstOrDefault()?.UpdateDate ?? DateTimeOffset.Now;
    public IEnumerable<ServerCountInfo> Cache => cache;

    public HistoryCountService(IServiceScopeFactory serviceScopeFactory, ILogger<HistoryCountService> logger)
    {
        _logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        try
        {
            Initialize();
        }
        catch (Exception e)
        {
            _logger.LogError("HistoryCountManager初始化失败 {Exception}", e);
        }
    }

    /// <summary>
    /// 初始化,缓存3天数据
    /// </summary>
    private void Initialize()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
        
        var day3 = DateTimeOffset.Now - TimeSpan.FromDays(3); //三天前
        var r = dbContext.ServerHistoryCountInfos.Where(v => v.UpdateDate > day3).AsNoTracking().ToArray();
        foreach (var item in r)
        {
            cache.Enqueue(item);
        }
        _logger.LogInformation("初始缓存个数:{CacheCount}", cache.Count);
    }

    private async Task AddAsync(ServerCountInfo info)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();

        var r = dbContext.ServerHistoryCountInfos.Add(info);
        await dbContext.SaveChangesAsync();

        cache.Enqueue(info);

        while (cache.Count > 10000)
        {
            cache.Dequeue();
        }

    }

    // data to info
    public Task AddAsync(ICollection<LobbyServerDetailed> data, DateTimeOffset updateTime)
    {
        LastUpdate = updateTime;

        ServerCountInfo countInfo = new();
        countInfo.UpdateDate = updateTime;
        countInfo.AllServerCount = data.Count;

        foreach (var item in data)
        {
            countInfo.AllPlayerCount += item.Connected;
            switch (item.Platform)
            {
                case Platform.Steam:
                    countInfo.SteamServerCount++;
                    countInfo.SteamPlayerCount += item.Connected;
                    break;
                case Platform.PlayStation:
                    countInfo.PlayStationServerCount++;
                    countInfo.PlayStationPlayerCount += item.Connected;
                    break;
                case Platform.WeGame or Platform.QQGame:
                    countInfo.WeGameServerCount++;
                    countInfo.WeGamePlayerCount += item.Connected;
                    break;
                case Platform.Xbox:
                    countInfo.XboxServerCount++;
                    countInfo.XboxPlayerCount += item.Connected;
                    break;
                case Platform.Switch:
                    countInfo.SwitchServerCount++;
                    countInfo.SwitchPlayerCount += item.Connected;
                    break;
            }
        }
        return AddAsync(countInfo);
    }

    /// <summary>
    /// 获取缓存的服务器历史数量信息
    /// </summary>
    /// <returns></returns>
    public ServerCountInfo[] GetServerHistory()
    {
        return cache.ToArray();
    }
}
