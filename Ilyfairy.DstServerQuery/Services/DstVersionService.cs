using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ilyfairy.Tools;
using Serilog;

namespace Ilyfairy.DstServerQuery.Services;

public class DstVersionService : IDisposable
{
    public long? Version { get; private set; }
    public CancellationTokenSource? TokenSource { get; private set; }
    private bool running = true;
    private DstDownloader? currentDst;
    private readonly ILogger log = Log.ForContext<DstVersionService>();
    public event EventHandler<long> VersionUpdated;
    private bool isDisposed = false;

    public async Task RunAsync(long? defaultVersion = null)
    {
        Version = defaultVersion;
        log.Information($"饥荒初始版本为 {defaultVersion}");

        await Task.Yield();

        while (running)
        {
            try
            {
                using DstDownloader dst = new();
                currentDst = dst;
                bool ok = false;
                TokenSource = new();

                //等待Login,设置超时时间
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(60), TokenSource.Token); //60秒超时
                    }
                    catch (Exception) { }
                    if (ok) return;
                    dst.Dispose();
                    if (TokenSource.IsCancellationRequested) return;
                    log.Warning("饥荒版本获取失败");
                    return;
                });

                try
                {
                    await dst.Steam.Authentication.LoginAnonymousAsync();
                }
                catch (Exception)
                {
                    log.Warning("Steam饥荒登录失败");
                    continue;
                }
                try
                {
                    var version = await dst.GetServerVersion();
                    VersionUpdated?.Invoke(this, version);
                    TokenSource.Cancel();
                    log.Information("饥荒版本获取成功: {0}", version);
                    Version = version;
                }
                catch (Exception)
                {
                    log.Warning("饥荒版本获取失败");
                }
            }
            catch { }
            await Task.Delay(10000).ConfigureAwait(false); //每10秒获取一次
        }
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        running = false;
        TokenSource?.Cancel();
        currentDst?.Steam.Disconnect();
    }
}
