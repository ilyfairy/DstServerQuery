using System.Diagnostics.CodeAnalysis;
using DstDownloaders;
using Microsoft.Extensions.Logging;
using SteamDownloader.Helpers;

namespace DstServerQuery.Services;

public class DstVersionService : IDisposable
{
    private DstDownloader? _currentDst;
    private readonly ILogger? _logger;

    public long? Version { get; private set; }
    public CancellationTokenSource CancellationTokenSource { get; private set; } = new();
    public CancellationTokenSource? CurrentCancellationTokenSource { get; private set; }
    public List<SteamContentServer> ContentServers { get; set; } = new();

    public event EventHandler<long>? VersionUpdated;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    public Func<DstDownloader> DstDownloaderFactory { get; set; } = () => new();

    public DstVersionService(ILogger? _logger = null)
    {
        this._logger = _logger;
    }

    private async Task CreateLoginAsync()
    {
        for (int i = 1; i <= 3; i++)
        {
            try
            {
                _currentDst = DstDownloaderFactory();
                await _currentDst.LoginAsync();
                break;
            }
            catch (Exception)
            {
                if (i == 3)
                    throw;
            }
        }
    }

    public async Task RunAsync(long? defaultVersion = null, bool disableUpdate = false)
    {
        Version = defaultVersion;
        _logger?.LogInformation("饥荒初始版本为 {DefaultVersion}", defaultVersion);

        if (disableUpdate)
            return;

        while (!CancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                _logger?.LogInformation("正在登录匿名Steam...");
                await CreateLoginAsync().ConfigureAwait(false);
                break;
            }
            catch (Exception)
            {
                _logger?.LogInformation("Steam登录失败, 正在重新登录...");
            }
        }

        if (_currentDst is null)
            throw new Exception("DstDownloader初始化失败");

        _logger?.LogInformation("Steam登录成功, 开始获取稳定的CDN服务器");
        {
            IEnumerable<SteamContentServer> tempServers = await _currentDst.Steam.GetCdnServersAsync().ConfigureAwait(false);
            try
            { tempServers = tempServers.Concat(await _currentDst.Steam.GetCdnServersAsync(1, cancellationToken: CancellationTokenSource.Token).ConfigureAwait(false)); }
            catch { }
            try
            { tempServers = tempServers.Concat(await _currentDst.Steam.GetCdnServersAsync(100, cancellationToken: CancellationTokenSource.Token).ConfigureAwait(false)); }
            catch { }
            try
            { tempServers = tempServers.Concat(await _currentDst.Steam.GetCdnServersAsync(150, cancellationToken: CancellationTokenSource.Token).ConfigureAwait(false)); }
            catch { }
            try
            { tempServers = tempServers.Concat(await _currentDst.Steam.GetCdnServersAsync(200, cancellationToken: CancellationTokenSource.Token).ConfigureAwait(false)); }
            catch { }
            var servers = tempServers.DistinctBy(v => v.SourceId).ToArray();

            await _currentDst.Steam.HttpClient.GetAsync(servers.First().Url, HttpCompletionOption.ResponseHeadersRead, CancellationTokenSource.Token); // 预热

            var stableServers = await SteamHelper.TestContentServerConnectionAsync(_currentDst.Steam.HttpClient, servers, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            ContentServers = stableServers.ToList();
            _currentDst.Steam.ContentServers = ContentServers;
            _logger?.LogInformation("Steam稳定服务器 ({StableCount}/{AllCount})", stableServers.Length, servers.Length);
        }

        while (!CancellationTokenSource.IsCancellationRequested)
        {
            CurrentCancellationTokenSource = new();

            CurrentCancellationTokenSource.CancelAfter(Timeout);
            try
            {
                var version = await _currentDst.GetServerVersionAsync(CurrentCancellationTokenSource.Token).ConfigureAwait(false);
                Version = version;
                VersionUpdated?.Invoke(this, version);
                CurrentCancellationTokenSource.Cancel();
                _logger?.LogInformation("饥荒版本获取成功: {Version}", version);
            }
            catch (OperationCanceledException)
            {
                CurrentCancellationTokenSource.Cancel();
                _logger?.LogWarning("获取饥荒版本超时");
                await CreateLoginAsync();
                continue;
            }
            catch (Exception e)
            {
                CurrentCancellationTokenSource.Cancel();
                _logger?.LogError(e, "饥荒版本获取失败");
                await CreateLoginAsync();
                continue;
            }
            await Task.Delay(Interval, CancellationTokenSource.Token).ConfigureAwait(false); //每30秒获取一次
        }
    }

    public void Dispose()
    {
        if (CancellationTokenSource.IsCancellationRequested) return;
        GC.SuppressFinalize(this);
        CancellationTokenSource.Cancel();
        CurrentCancellationTokenSource?.Cancel();
        _currentDst?.Dispose();
    }
}
