using DstDownloaders;
using DstDownloaders.Mods;
using DstServerQuery.EntityFrameworkCore;
using DstServerQuery.EntityFrameworkCore.Model;
using DstServerQuery.Helpers;
using DstServerQuery.Models.Requests;
using DstServerQuery.Services;
using DstServerQuery.Web.Helpers;
using DstServerQuery.Web.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DstServerQuery.Web.Services;

public class AppHostedService(ILogger<AppHostedService> _logger,
                              IServiceProvider _serviceProvider,
                              IConfiguration _configuration,
                              DstWebConfig _dstWebConfig,
                              SteamOptions _steamOptions,
                              DstVersionService _dstVersionService,
                              DstVersionServiceOptions _dstVersionServiceOptions,
                              DstModsFileServiceOptions _dstModsFileServiceOptions,
                              HistoryCountService _historyCountService,
                              LobbyServerManager _lobbyServerManager,
                              GeoIPService _geoIPService) : IHostedService
{
    private DstModsFileService? _dstModsFileService;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _dstModsFileService = _serviceProvider.GetService<DstModsFileService>();

        using var scope = _serviceProvider.CreateScope();
        _logger.LogInformation("IHostApplicationBuilder Start");

        SimpleCacheDatabase simpleCacheDatabase = scope.ServiceProvider.GetRequiredService<SimpleCacheDatabase>();

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
            _logger.LogInformation("数据库迁移成功");
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogInformation("数据库创建成功");
        }

        try
        {
            await _historyCountService.Initialize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HistoryCountManager初始化失败");
        }


        // 配置GeoIP
        if (_configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
        {
            try
            {
                _geoIPService.Initialize(geoLite2Path);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GeoIP初始化异常");
                return;
            }
            DstConverterHelper.GeoIPService = _geoIPService;
        }

        // 启动服务管理器
        await _lobbyServerManager.Start();

        // 饥荒版本获取服务
        _dstVersionService.DstDownloaderFactory = () =>
        {
            return new DstDownloader(Helper.CreateSteamSession(_serviceProvider));
        };
        var defaultVersion = simpleCacheDatabase.Get<long?>("DstVersion") ?? _dstVersionServiceOptions.DefaultVersion;
        _ = _dstVersionService.RunAsync(defaultVersion, _dstVersionServiceOptions.IsDisabledUpdate);
        _dstVersionService.VersionUpdated += (sender, version) =>
        {
            using var scope = _serviceProvider.CreateScope();
            using var simpleCacheDatabase = scope.ServiceProvider.GetRequiredService<SimpleCacheDatabase>();
            simpleCacheDatabase["DstVersion"] = version;
        };

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IHostApplicationBuilder Shutdowning");

        _dstVersionService.Dispose();
        _lobbyServerManager.Dispose();
        _dstModsFileService?.Dispose();

        Serilog.Log.CloseAndFlush();
    }
}
