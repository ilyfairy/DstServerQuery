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
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(60);

    public Func<DstDownloader> DstDownloaderFactory { get; set; } = () => new();

    public DstVersionService(ILogger? _logger = null)
    {
        this._logger = _logger;
    }

    private async Task CreateLoginAsync(CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 3; i++)
        {
            DstDownloader? dst = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                dst = DstDownloaderFactory();
                using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutTokenSource.CancelAfter(Timeout);

                await dst.LoginAsync(timeoutTokenSource.Token).ConfigureAwait(false);
                if (ContentServers.Count > 0)
                {
                    dst.Steam.ContentServers = ContentServers.ToList();
                }

                var oldDst = _currentDst;
                _currentDst = dst;
                oldDst?.Dispose();
                return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                dst?.Dispose();
                if (i == 3)
                    throw new TimeoutException($"Steam匿名登录超过 {Timeout.TotalSeconds:0.##} 秒");
            }
            catch (Exception)
            {
                dst?.Dispose();
                if (i == 3)
                    throw;
            }
        }
    }

    private async Task RefreshContentServersAsync(CancellationToken cancellationToken)
    {
        if (_currentDst is null)
            throw new InvalidOperationException("DstDownloader未初始化");

        _logger?.LogInformation("Steam登录成功, 开始获取稳定的CDN服务器");

        using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutTokenSource.CancelAfter(Timeout);
        var timeoutToken = timeoutTokenSource.Token;

        IEnumerable<SteamContentServer> tempServers = await _currentDst.Steam.GetCdnServersAsync(cancellationToken: timeoutToken).ConfigureAwait(false);

        foreach (var cellId in new uint[] { 1, 100, 150, 200 })
        {
            try
            {
                tempServers = tempServers.Concat(await _currentDst.Steam.GetCdnServersAsync(cellId, cancellationToken: timeoutToken).ConfigureAwait(false));
            }
            catch
            {
                if (cancellationToken.IsCancellationRequested)
                    throw;
            }
        }

        var servers = tempServers.DistinctBy(v => v.SourceId).ToArray();
        if (servers.Length == 0)
            throw new InvalidOperationException("未获取到Steam CDN服务器");

        var stableServers = await SteamHelper.TestContentServerConnectionAsync(_currentDst.Steam.HttpClient, servers, TimeSpan.FromSeconds(5), timeoutToken).ConfigureAwait(false);
        if (stableServers.Length == 0)
            throw new InvalidOperationException($"Steam CDN服务器全部不可用 ({servers.Length})");

        ContentServers = stableServers.ToList();
        _currentDst.Steam.ContentServers = ContentServers.ToList();
        _logger?.LogInformation("Steam稳定服务器 ({StableCount}/{AllCount})", stableServers.Length, servers.Length);
    }

    private async Task ResetSteamSessionAsync(CancellationToken cancellationToken)
    {
        _logger?.LogWarning("正在重新登录Steam并刷新CDN服务器");

        _currentDst?.Dispose();
        _currentDst = null;

        await CreateLoginAsync(cancellationToken).ConfigureAwait(false);
        await RefreshContentServersAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RunAsync(long? defaultVersion = null, bool disableUpdate = false)
    {
        Version = defaultVersion;
        _logger?.LogInformation("饥荒初始版本为 {DefaultVersion}", defaultVersion);

        if (disableUpdate)
        {
            _logger?.LogInformation("饥荒版本获取服务已禁用");
            return;
        }

        var cancellationToken = CancellationTokenSource.Token;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_currentDst is null)
                {
                    await ResetSteamSessionAsync(cancellationToken).ConfigureAwait(false);
                }

                using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                CurrentCancellationTokenSource = timeoutTokenSource;
                timeoutTokenSource.CancelAfter(Timeout);

                bool isSuccess = false;
                try
                {
                    var version = await _currentDst!.GetServerVersionAsync(timeoutTokenSource.Token).ConfigureAwait(false);
                    Version = version;
                    _logger?.LogInformation("饥荒版本获取成功: {Version}", version);
                    isSuccess = true;

                    try
                    {
                        VersionUpdated?.Invoke(this, version);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "饥荒版本更新事件处理失败");
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogWarning("获取饥荒版本超时");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "饥荒版本获取失败");
                }
                finally
                {
                    if (ReferenceEquals(CurrentCancellationTokenSource, timeoutTokenSource))
                        CurrentCancellationTokenSource = null;
                }

                if (!isSuccess)
                {
                    await ResetSteamSessionAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "饥荒版本获取服务循环异常, 将继续重试");
                _currentDst?.Dispose();
                _currentDst = null;
            }


            try
            {
                await Task.Delay(Interval, cancellationToken).ConfigureAwait(false); // 默认每1分钟获取一次
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        _logger?.LogInformation("饥荒版本获取服务已停止");
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
