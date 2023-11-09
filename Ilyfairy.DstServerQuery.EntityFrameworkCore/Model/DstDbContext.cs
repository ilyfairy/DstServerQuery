using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using Ilyfairy.DstServerQuery.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ilyfairy.DstServerQuery.Models;

public class DstDbContext : DbContext
{
    public DbSet<ServerCountInfo> ServerHistoryCountInfos { get; set; }
    public DbSet<DstPlayer> Players { get; set; }
    public DbSet<DstServerHistory> ServerHistories { get; set; }
    public DbSet<DstServerHistoryItem> ServerHistoryItems { get; set; }
    public DbSet<HistoryServerItemPlayer> HistoryServerItemPlayerPair { get; set; }
    public DbSet<DstDaysInfo> DaysInfos { get; set; }

    public DstDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //全局禁用跟踪查询
        //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
#if DEBUG
        //显示更详细的日志
        //optionsBuilder.EnableDetailedErrors();
#endif

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var provider = Database.ProviderName ?? "";
        if (provider.Contains("sqlserver", StringComparison.OrdinalIgnoreCase))
        {
            modelBuilder.UseCollation("Chinese_PRC_BIN");
        }
        else if (provider.Contains("mysql", StringComparison.OrdinalIgnoreCase))
        {
            modelBuilder.UseCollation("utf8mb4_bin");
        }


        //服务器信息和历史记录信息的一对多
        modelBuilder.Entity<DstServerHistory>()
            .HasMany(v => v.Items)
            .WithOne(v => v.Server)
            .HasForeignKey(v => v.ServerId)
            .IsRequired();

        //历史记录信息和天数信息的一对一
        modelBuilder.Entity<DstServerHistoryItem>()
            .HasOne(v => v.DaysInfo)
            .WithOne(v => v.ServerItem)
            .HasForeignKey<DstServerHistoryItem>(v => v.DaysInfoId);

        //历史记录信息和玩家信息多对多
        modelBuilder.Entity<DstServerHistoryItem>()
            .HasMany(v => v.Players)
            .WithMany(v => v.ServerHistoryItems)
            .UsingEntity<HistoryServerItemPlayer>( // OnDelete禁用联级删除
                l => l.HasOne(v => v.Player).WithMany().HasForeignKey(v => v.PlayerId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(v => v.HistoryServerItem).WithMany().HasForeignKey(v => v.HistoryServerItemId).OnDelete(DeleteBehavior.Restrict)
            );
    }

}
