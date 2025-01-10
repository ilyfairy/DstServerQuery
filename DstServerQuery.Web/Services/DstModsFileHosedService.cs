using DstDownloaders.Mods;
using DstServerQuery.Web.Helpers;
using DstServerQuery.Web.Models.Configurations;

namespace DstServerQuery.Web.Services;

public class DstModsFileHosedService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly ILogger<DstModsFileService> _logger = serviceProvider.GetRequiredService<ILogger<DstModsFileService>>();
    private readonly DstModsFileServiceOptions _options = serviceProvider.GetRequiredService<DstModsFileServiceOptions>();
    private readonly DstModsFileService _service = serviceProvider.GetRequiredService<DstModsFileService>();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.IsEnabled)
            return Task.CompletedTask;

        _ = Run();

        return Task.CompletedTask;
    }

    public async Task Run()
    {
        _service.StringCacheConverter = new();
        _service.IsEnableMultiLanguage = _options.IsEnableMultiLanguage;

        _ = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(7));
                _service.StringCacheConverter.Cache.Clear(); // 每7天清理字符串缓存
            }
        });

        // 饥荒Mods服务
        _logger.LogInformation("DstModsFileService Root: {Root}", _options.RootPath);
        _logger.LogInformation("DstModsFileService FileProxyUrl: {Url}", _options.FileUrlProxy);
        await _service.InitializeAsync(async dst =>
        {
            await dst.LoginAsync();
            await Helper.EnsureContentServerAsync(dst, _cancellationTokenSource.Token);
        });
        _logger.LogInformation("DstModsFileService 登录成功");
        _service.EnsureCache();
        _logger.LogInformation("DstModsFileService 一共加载了{Count}个Mods", _service.Cache.Count);

        _ = UpdateLoop();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();

        return Task.CompletedTask;
    }

    public async Task UpdateLoop()
    {
        await Task.Yield();

        if (_service.Cache.Count != 0)
        {
            try
            {
                _logger.LogInformation("DstModsFileService 开始更新所有Mods的SteamInfo");
                await _service.RunUpdateSteamInfoAsync();
                _logger.LogInformation("DstModsFileService 更新所有Mods的SteamInfo成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DstModsFileService 更新所有Mods的SteamInfo失败");
            }
        }

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (_service.Cache.Count != 0)
            {
                try
                {
                    _logger.LogInformation("DstModsFileService 开始更新多语言描述");
                    await _service.RunUpdateMultiLanguageDescriptionAsync(_cancellationTokenSource.Token);
                    _logger.LogInformation("DstModsFileService 多语言描述更新完成");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DstModsFileService 多语言描述更新异常");
                }
            }

            try
            {
                _logger.LogInformation("DstModsFileService 开始更新所有Mods");

                CancellationTokenSource timeoutCts = new();
                CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, timeoutCts.Token);
                DateTimeOffset lastUpdateTime = DateTimeOffset.Now;

                await _service.RunUpdateAllAsync(v =>
                {
                    //检测超时
                    lastUpdateTime = DateTimeOffset.Now;
                    Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(task =>
                    {
                        if (DateTimeOffset.Now - lastUpdateTime > TimeSpan.FromMinutes(5))
                        {
                            timeoutCts.Cancel();
                        }
                    });

                    //log
                    if (v.Type is DstModsFileService.ModsUpdateType.Valid or DstModsFileService.ModsUpdateType.UpdateInfo)
                        return;

                    if (v.Type is DstModsFileService.ModsUpdateType.Failed)
                    {
                        _logger.LogWarning("DstModsFileService  下载Mod失败  {WorkshopId,-10}  用时:{Time:0.000}s  {Name}  {Exception}"
                            , v.WorkshopId, v.UpdateElapsed.TotalMilliseconds / 1000.0, v.Store?.SteamModInfo?.Name, v.Exception?.Message);
                    }
                    else
                    {
                        var typeString = v.Type switch
                        {
                            //DstModsFileService.ModsUpdateType.Valid => "验证Mod成功",
                            DstModsFileService.ModsUpdateType.Update => "更新Mod成功",
                            DstModsFileService.ModsUpdateType.Download => "下载Mod成功",
                            //DstModsFileService.ModsUpdateType.Failed => $"下载Mod失败   IsConnected:{service.DstSession.Steam.SteamClient.IsConnected}   ",
                            _ => string.Empty,
                        };
                        _logger.LogInformation("DstModsFileService  {Type}  {WorkshopId,-10}  用时:{Time:0.000}s  {Name}"
                            , typeString, v.WorkshopId, v.UpdateElapsed.TotalMilliseconds / 1000.0, v.Store?.SteamModInfo?.Name);
                    }
                }, linked.Token);
                _logger.LogInformation("DstModsFileService 所有Mods更新完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DstModsFileService 更新Mods异常");
            }
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}
