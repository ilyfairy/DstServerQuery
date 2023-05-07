using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ilyfairy.Tools;
using NLog;

namespace Ilyfairy.DstServerQuery.Utils;

public class DstVersionGetter
{
    public long Version { get; private set; }
    public CancellationTokenSource? TokenSource { get; private set; }
    private bool running = true;
    private DstDownloader? currentDst;
    private readonly Logger log = LogManager.GetLogger("DstVersionGetter");

    public void Start(long defaultVersion = 0)
    {
        Version = defaultVersion;
        log.Info($"饥荒初始版本为 {defaultVersion}");

        Task.Run(async () =>
        {
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
                        log.Info("饥荒版本获取失败");
                        return;
                    });

                    if (dst.Login())
                    {
                        var version = await dst.GetServerVersion();
                        if (version.HasValue && version.Value > 0)
                        {
                            this.Version = version.Value;
                            ok = true;
                            TokenSource.Cancel();
                            log.Info("饥荒版本获取成功: {0}", version);
                        }
                        else
                        {
                            log.Info("饥荒版本获取失败");
                        }
                    }
                }
                catch
                {

                }
                await Task.Delay(10000); //每10秒获取一次
            }
        });
    }

    public void Abort()
    {
        running = false;
        TokenSource?.Cancel();
        currentDst?.Disconnect();
    }
}
