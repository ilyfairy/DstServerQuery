using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore;

/// <summary>
/// 大厅服务器历史房间数量管理器
/// </summary>
public class HistoryCountManager
{
    private readonly Logger logger = LogManager.GetLogger("DstServerQuery.HistoryCountManager");
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly Queue<ServerCountInfo> cache = new(10100);

    public DateTime LastUpdate { get; private set; }
    public DateTime First => cache.FirstOrDefault()?.UpdateDate ?? DateTime.Now;
    public IEnumerable<ServerCountInfo> Cache => cache;

    public HistoryCountManager(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        try
        {
            Initialize();
        }
        catch (Exception)
        {
            logger.Error("HistoryCountManager初始化失败");
        }
    }

    /// <summary>
    /// 初始化,缓存3天数据
    /// </summary>
    private void Initialize()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
        
        var day3 = DateTime.Now - TimeSpan.FromDays(3); //三天前
        var r = dbContext.ServerHistoryCountInfos.Where(v => v.UpdateDate > day3).ToArray();
        foreach (var item in r)
        {
            cache.Enqueue(item);
        }
        logger.Info($"初始缓存个数:{cache.Count}");
    }

    private bool Add(ServerCountInfo info)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();

        var r = dbContext.ServerHistoryCountInfos.Add(info);
        try
        {
            dbContext.SaveChanges();
        }
        catch (Exception)
        {
            return false;
        }
        cache.Enqueue(info);

        while (cache.Count > 10000)
        {
            cache.Dequeue();
        }

        return r.State == EntityState.Added; // 修改是否成功
    }

    // data to info
    public bool Add(ICollection<LobbyServerDetailed> data, DateTime updateTime)
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
        return Add(countInfo);
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
