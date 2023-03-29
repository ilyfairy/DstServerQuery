using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore
{
    /// <summary>
    /// 大厅服务器历史房间数量管理器
    /// </summary>
    public class HistoryCountManager
    {
        private readonly Func<DstDbContext> dbContext;
        private readonly Logger logger = LogManager.GetLogger("DstServerQuery.HistoryCountManager");
        private readonly Queue<ServerCountInfo> cache = new(10000);

        public HistoryCountManager(Func<DstDbContext> dbContext)
        {
            this.dbContext = dbContext;
            try
            {
                Initialize();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 初始化,缓存3天数据
        /// </summary>
        private void Initialize()
        {
            using var db = dbContext();
            var day3 = DateTime.Now - TimeSpan.FromDays(3); //三天前
            var r = db.ServerHistoryCountInfos.Where(v => v.UpdateDate > day3);
            foreach (var item in r)
            {
                cache.Enqueue(item);
            }
            logger.Info($"初始缓存个数:{cache.Count}");
        }

        public bool Add(ServerCountInfo info)
        {
            using var db = dbContext();
            var r = db.ServerHistoryCountInfos.Add(info);
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            cache.Enqueue(info);

            if(cache.Count > 10000)
            {
                for (int i = 0; i < 10; i++)
                {
                    cache.Dequeue();
                }
            }

            return r.State == EntityState.Added;
        }

        public bool Add(ICollection<LobbyDetailsData> data, DateTime updateTime)
        {
            ServerCountInfo countInfo = new();
            countInfo.UpdateDate = updateTime;
            countInfo.AllServerCount = data.Count;
            countInfo.AllPlayerCount = data.Select(v => v.Connected).Sum();

            countInfo.SteamServerCount = data.Where(v => v.Platform == Platform.Steam).Count();
            countInfo.WeGameServerCount = data.Where(v => v.Platform == Platform.WeGame).Count();
            countInfo.PlayStationServerCount = data.Where(v => v.Platform == Platform.PlayStation).Count();
            countInfo.XboxServerCount = data.Where(v => v.Platform == Platform.Xbox).Count();
            countInfo.SwitchServerCount = data.Where(v => v.Platform == Platform.Switch).Count();

            countInfo.SteamPlayerCount = data.Where(v => v.Platform == Platform.Steam).Select(p => p.Connected).Sum();
            countInfo.WeGamePlayerCount = data.Where(v => v.Platform == Platform.WeGame).Select(p => p.Connected).Sum();
            countInfo.PlayStationPlayerCount = data.Where(v => v.Platform == Platform.PlayStation).Select(p => p.Connected).Sum();
            countInfo.XboxPlayerCount = data.Where(v => v.Platform == Platform.Xbox).Select(p => p.Connected).Sum();
            countInfo.SwitchPlayerCount = data.Where(v => v.Platform == Platform.Switch).Select(p => p.Connected).Sum();
            return Add(countInfo);
        }

        /// <summary>
        /// 获取缓存的服务器历史数量信息
        /// </summary>
        /// <returns></returns>
        public List<ServerCountInfo> GetServerHistory()
        {
            return cache.ToList();
        }
    }
}
