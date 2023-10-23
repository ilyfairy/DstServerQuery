using Ilyfairy.DstServerQuery.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ilyfairy.DstServerQuery.Models;

public class DstDbContext : DbContext
{
    public DbSet<ServerCountInfo> ServerHistoryCountInfos { get; set; }

    public DstDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //全局禁用跟踪查询
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
#if DEBUG
        //显示更详细的日志
        optionsBuilder.EnableDetailedErrors();
#endif

    }

}
