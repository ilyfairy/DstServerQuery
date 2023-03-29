using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Diagnostics;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore
{
    public class UserRequestRecordManager
    {
        private readonly Func<DstDbContext> dbContext;
        private readonly Logger logger = LogManager.GetLogger("DstServerQuery.UserRequestRecordManager");

        public UserRequestRecordManager(Func<DstDbContext> dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// 添加请求记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> AddRequestRecord(UserRequestRecord data)
        {
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            try
            {
                using var db = dbContext();
                var r = await db.UserRequestRecords.AddAsync(data, cts.Token);
                db.SaveChanges();
                return r.State == EntityState.Added;
            }
            catch (Exception e)
            {
                logger.Error($"添加失败 {e.Message}");
                return false;
            }
        }


    }
}
