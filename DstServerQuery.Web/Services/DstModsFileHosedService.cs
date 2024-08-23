using DstDownloaders.Mods;
using Ilyfairy.DstServerQuery.Web.Helpers;
using Ilyfairy.DstServerQuery.Web.Models.Configurations;

namespace Ilyfairy.DstServerQuery.Web.Services;

public class DstModsFileHosedService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly ILogger<DstModsFileService> logger = serviceProvider.GetRequiredService<ILogger<DstModsFileService>>();
    private readonly DstModsFileServiceOptions options = serviceProvider.GetRequiredService<DstModsFileServiceOptions>();
    private readonly DstModsFileService service = serviceProvider.GetRequiredService<DstModsFileService>();

    private readonly CancellationTokenSource cts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.IsEnabled)
            return Task.CompletedTask;

        _ = Run();

        return Task.CompletedTask;
    }

    public async Task Run()
    {
        // 饥荒Mods服务
        logger.LogInformation("DstModsFileService Root: {Root}", options.RootPath);
        logger.LogInformation("DstModsFileService FileProxyUrl: {Url}", options.FileUrlProxy);
        await service.InitializeAsync(async dst =>
        {
            await dst.LoginAsync(cts.Token);
            await Helper.EnsureContentServerAsync(dst, cts.Token);
        });
        logger.LogInformation("DstModsFileService 登录成功");
        service.EnsureCache();
        logger.LogInformation("DstModsFileService 一共加载了{Count}个Mods", service.Cache.Count);

        _ = UpdateLoop();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();

        return Task.CompletedTask;
    }

    public async Task UpdateLoop()
    {
        await Task.Yield();

        try
        {
            logger.LogInformation("DstModsFileService 开始更新所有Mods的SteamInfo");
            await service.RunUpdateSteamInfoAsync();
            logger.LogInformation("DstModsFileService 更新所有Mods的SteamInfo成功");
        }
        catch (Exception)
        {
            logger.LogError("DstModsFileService 更新所有Mods的SteamInfo失败");
        }

        while (!cts.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("DstModsFileService 开始更新多语言描述");
                await service.RunUpdateMultiLanguageDescriptionAsync(cts.Token);
                logger.LogInformation("DstModsFileService 多语言描述更新完成");
            }
            catch (Exception ex)
            {
                logger.LogError("DstModsFileService 多语言描述更新异常: {Exception}", ex);
            }

            try
            {
                logger.LogInformation("DstModsFileService 开始更新所有Mods");

                CancellationTokenSource timeoutCts = new();
                CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);
                DateTimeOffset lastUpdateTime = DateTimeOffset.Now;

                await service.RunUpdateAllAsync(v =>
                {
                    //检测超时
                    lastUpdateTime = DateTimeOffset.Now;
                    Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(task =>
                    {
                        if(DateTimeOffset.Now - lastUpdateTime > TimeSpan.FromMinutes(5))
                        {
                            timeoutCts.Cancel();
                        }
                    });

                    //log
                    if (v.Type is DstModsFileService.ModsUpdateType.Valid or DstModsFileService.ModsUpdateType.UpdateInfo)
                        return;

                    if (v.Type is DstModsFileService.ModsUpdateType.Failed)
                    {
                        logger.LogInformation("DstModsFileService  下载Mod失败  {WorkshopId,-10}  用时:{Time:0.000}s  {Name}  {Exception}"
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
                        logger.LogInformation("DstModsFileService  {Type}  {WorkshopId,-10}  用时:{Time:0.000}s  {Name}"
                            , typeString, v.WorkshopId, v.UpdateElapsed.TotalMilliseconds / 1000.0, v.Store?.SteamModInfo?.Name);
                    }
                }, linked.Token);
                logger.LogInformation("DstModsFileService 所有Mod更新完成");
            }
            catch (Exception ex)
            {
                logger.LogError("DstModsFileService 更新所有Mods异常 {Exception}", ex);
            }
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}
