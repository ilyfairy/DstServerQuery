using DstDownloaders;
using DstDownloaders.Mods;
using DstServerQuery.EntityFrameworkCore.Model;
using DstServerQuery.Helpers;
using DstServerQuery.Models.Requests;
using DstServerQuery.Services;
using DstServerQuery.Web.Helpers;
using DstServerQuery.Web.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DstServerQuery.Web.Services;

public class AppHostedService(ILogger<AppHostedService> logger,
                              IServiceProvider serviceProvider,
                              IConfiguration configuration,
                              SimpleCacheDatabase simpleCacheDatabase,
                              DstWebConfig dstWebConfig,
                              SteamOptions steamOptions,
                              DstVersionService dstVersionService,
                              DstVersionServiceOptions dstVersionServiceOptions,
                              DstModsFileServiceOptions dstModsFileServiceOptions,
                              LobbyServerManager lobbyServerManager,
                              GeoIPService geoIPService,
                              DstModsFileService dstModsFileService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostApplicationBuilder>>();
        logger.LogInformation("IHostApplicationBuilder Start");

        // 键值对缓存数据库
        simpleCacheDatabase.EnsureInitialize();

        // 数据库迁移
        using var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
        bool isMigration = false;
        try
        {
            isMigration = dbContext.Database.GetPendingMigrations().Any();
        }
        catch { }
        if (isMigration)
        {
            dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(100));
            await dbContext.Database.MigrateAsync(cancellationToken); //执行迁移
            logger.LogInformation("数据库迁移成功");
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("数据库创建成功");
        }

        // 配置GeoIP
        if (configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
        {
            geoIPService.Initialize(geoLite2Path);
            DstConverterHelper.GeoIPService = geoIPService;
        }

        // 启动服务管理器
        await lobbyServerManager.Start();

        // 饥荒版本获取服务
        dstVersionService.DstDownloaderFactory = () =>
        {
            return new DstDownloader(Helper.CreateSteamSession(serviceProvider));
        };
        var defaultVersion = simpleCacheDatabase.Get<long?>("DstVersion") ?? dstVersionServiceOptions.DefaultVersion;
        _ = dstVersionService.RunAsync(defaultVersion, dstVersionServiceOptions.IsDisabledUpdate);
        dstVersionService.VersionUpdated += (sender, version) =>
        {
            simpleCacheDatabase["DstVersion"] = version;
        };
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("IHostApplicationBuilder Shutdowning");

        dstVersionService.Dispose();
        lobbyServerManager.Dispose();
        dstModsFileService?.Dispose();

        Serilog.Log.CloseAndFlush();
    }
}
