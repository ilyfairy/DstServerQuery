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
                    await Task.Delay(TimeSpan.FromSeconds(60), TokenSource.Token); //60秒超时
                    if (ok) return;
                    dst.Dispose();
                    log.Warning("饥荒版本获取失败");
                    return;
                });

                if (dst.Login())
                {
                    var version = await dst.GetServerVersion();
                    if (version.HasValue && version.Value > 0)
                    {
                        Version = version.Value;
                        ok = true;
                        TokenSource.Cancel();
                        log.Information("饥荒版本获取成功: {0}", version);
                    }
                    else
                    {
                        log.Warning("饥荒版本获取失败");
                    }
                }
            }
            catch { }
            await Task.Delay(10000).ConfigureAwait(false); //每10秒获取一次
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        running = false;
        TokenSource?.Cancel();
        currentDst?.Disconnect();
    }
}
