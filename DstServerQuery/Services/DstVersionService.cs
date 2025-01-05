using System.Diagnostics.CodeAnalysis;
using DstDownloaders;
using Microsoft.Extensions.Logging;
using SteamDownloader.Helpers;

namespace DstServerQuery.Services;

public class DstVersionService : IDisposable
{
    public long? Version { get; private set; }
    public CancellationTokenSource? CurrentCancellationTokenSource { get; private set; }
    private bool running;
    private DstDownloader? currentDst;
    private readonly ILogger _logger;
    public event EventHandler<long>? VersionUpdated;
    private bool isDisposed = false;
    public List<SteamContentServer> ContentServers { get; set; } = new();

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    public Func<DstDownloader> DstDownloaderFactory { get; set; } = () => new();

    [MemberNotNull(nameof(currentDst))]
    private async Task LoginAsync()
    {
        currentDst = DstDownloaderFactory();
        await currentDst.LoginAsync();
    }

    public async Task RunAsync(long? defaultVersion = null, bool disableUpdate = false, ILogger? _logger = null)
    {
        Version = defaultVersion;
        _logger?.LogInformation("饥荒初始版本为 {DefaultVersion}", defaultVersion);

        if (disableUpdate)
            return;

        while (true)
        {
            try
            {
                _logger?.LogInformation("正在登录匿名Steam...");
                await LoginAsync();
                break;
            }
            catch (Exception)
            {
                _logger?.LogInformation("Steam登录失败, 正在重新登录...");
            }
        }

        _logger?.LogInformation("Steam登录成功, 开始获取稳定的CDN服务器");
        {
            IEnumerable<SteamContentServer> tempServers = await currentDst.Steam.GetCdnServersAsync().ConfigureAwait(false);
            tempServers = tempServers.Concat(await currentDst.Steam.GetCdnServersAsync(1).ConfigureAwait(false));
            tempServers = tempServers.Concat(await currentDst.Steam.GetCdnServersAsync(100).ConfigureAwait(false));
            tempServers = tempServers.Concat(await currentDst.Steam.GetCdnServersAsync(150).ConfigureAwait(false));
            tempServers = tempServers.Concat(await currentDst.Steam.GetCdnServersAsync(200).ConfigureAwait(false));
            var servers = tempServers.DistinctBy(v => v.SourceId).ToArray();

            await currentDst.Steam.HttpClient.GetAsync(servers.First().Url, HttpCompletionOption.ResponseHeadersRead); // 预热

            var stableServers = await SteamHelper.TestContentServerConnectionAsync(currentDst.Steam.HttpClient, servers, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            ContentServers = stableServers.ToList();
            currentDst.Steam.ContentServers = ContentServers;
            _logger?.LogInformation("Steam稳定服务器 ({StableCount}/{AllCount})", stableServers.Length, servers.Length);
        }


        running = true;
        int processedCount = 0;
        while (running)
        {
            CurrentCancellationTokenSource = new();
            var currentProcessedCount = processedCount;
            CurrentCancellationTokenSource.Token.Register(async () =>
            {
                await Task.Delay(500);
                if (currentProcessedCount == processedCount)
                {
                    _logger?.LogInformation("Steam会话无响应状态");
                }
            });

            CurrentCancellationTokenSource.CancelAfter(Timeout);
            try
            {
                var version = await currentDst.GetServerVersionAsync(CurrentCancellationTokenSource.Token).ConfigureAwait(false);
                VersionUpdated?.Invoke(this, version);
                processedCount++;
                Version = version;
                CurrentCancellationTokenSource.Cancel();
                _logger?.LogInformation("饥荒版本获取成功: {Version}", version);
            }
            catch (OperationCanceledException)
            {
                processedCount++;
                _logger?.LogInformation("获取饥荒版本已超时");
                continue;
            }
            catch (Exception e)
            {
                processedCount++;
                _logger?.LogError(e, "饥荒版本获取失败");
                continue;
            }
            await Task.Delay(Interval).ConfigureAwait(false); //每30秒获取一次
        }
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        running = false;
        CurrentCancellationTokenSource?.Cancel();
        currentDst?.Steam.Disconnect();
    }
}
