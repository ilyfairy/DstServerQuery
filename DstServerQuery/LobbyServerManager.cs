using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using DstServerQuery.Models.Requests;
using DstServerQuery.Models.Lobby;
using Microsoft.Extensions.Logging;

namespace DstServerQuery;

/// <summary>
/// 大厅的详细信息实时获取以及管理
/// </summary>
public class LobbyServerManager : IDisposable
{
    private readonly ILogger? _logger;
    public ConcurrentDictionary<string, LobbyServerDetailed> ServerMap { get; } = new(2, 40000);
    private ICollection<LobbyServerDetailed> _serverCache = [];

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly DstWebConfig _dstConfig;

    public bool Running => !HttpCancellationToken.IsCancellationRequested;
    public LobbyDownloader LobbyDownloader { get; private set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdate { get; private set; }

    public CancellationTokenSource HttpCancellationToken { get; private set; } = new(0); // 初始为取消状态

    public event EventHandler<DstUpdatedEventArgs>? ServerUpdated;
    public event EventHandler<DstUpdatedDetailsChunk>? DetailsChunkUpdated;

    public bool ModifyEnabled { get; set; } = true;

    //参数是依赖注入
    public LobbyServerManager(DstWebConfig requestConfig, LobbyDownloader lobbyDownloader, ILogger<LobbyServerManager>? logger = null)
    {
        LobbyDownloader = lobbyDownloader;
        _logger = logger;
        _dstConfig = requestConfig;
    }


    public async Task Start()
    {
        HttpCancellationToken = new();

        //重试
        for (int i = 1; i <= 3; i++)
        {
            try
            {
                await LobbyDownloader.InitializeAsync();
                break;
            }
            catch (Exception)
            {
                if (i >= 3)
                    throw;
            }
        }

        _logger?.LogInformation("开始DownloadLoop Task");
        _stopwatch.Restart();

        //循环获取新版服务器数据
        _ = Task.Run(async () =>
        {
            try
            {
                await RequestLoop();
            }
            catch (Exception e)
            {
                HttpCancellationToken.Cancel();
                _logger?.LogError("DownloadLoopException: {Exception}", e.Message);
            }
        });

        //循环更新详细数据
        _ = UpdatingLoop();
    }

    public void Dispose()
    {
        HttpCancellationToken.Cancel();
        GC.SuppressFinalize(this);
        _logger?.LogInformation("LobbyDetailsManager Dispose");
    }

    //循环获取数据  
    private async Task RequestLoop()
    {
        List<string> newRowIdLst = new();
        //Dictionary<string, LobbyServerRaw> tempServerRowIdMap = new(40000);
        while (Running)
        {
            bool modifyEnabled = ModifyEnabled;
            List<LobbyServer>? unchanged = modifyEnabled ? new(10000) : null;
            List<LobbyServer>? added = modifyEnabled ? new(1000) : null;
            try
            {
                newRowIdLst.Clear();
                CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromMinutes(3));
                _logger?.LogInformation("开始 Download");

                await foreach (var item in LobbyDownloader.DownloadAllBriefs(CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpCancellationToken.Token).Token))
                {
                    newRowIdLst.Add(item.RowId);

                    //重复的
                    if (ServerMap.TryGetValue(item.RowId, out var current))
                    {
                        LobbyServer server = ServerMap[item.RowId];
                        server.UpdateFrom(item);
                        if (modifyEnabled)
                            unchanged!.Add(server);
                    }
                    //新增的
                    else
                    {
                        LobbyServer server = new LobbyServerDetailed();
                        server.UpdateFrom(item as LobbyServerRaw);
                        if (ServerMap.TryAdd(item.RowId, (server as LobbyServerDetailed)!))
                        {
                            if (modifyEnabled)
                                added!.Add(server);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                if (!Running || HttpCancellationToken.IsCancellationRequested)
                {
                    _logger?.LogWarning("中断请求,结束");
                    break;
                }
                _logger?.LogWarning($"请求失败,重新请求\n{e.Message}");
                continue;
            }

            if (!Running) break;

            LastUpdate = DateTimeOffset.Now;
            var currentRowIds = ServerMap.Keys;

            //移除的
            List<LobbyServer>? removed = modifyEnabled ? new(1000) : null;
            foreach (var rowId in currentRowIds.Except(newRowIdLst))
            {
                if (ServerMap.TryRemove(rowId, out var rm))
                {
                    if (modifyEnabled)
                        removed!.Add(rm);
                }
            }

            _serverCache = new List<LobbyServerDetailed>(ServerMap.Values);

            ServerUpdated?.Invoke(this, new DstUpdatedEventArgs(_serverCache, DateTimeOffset.Now)
            {
                UnchangedServers = unchanged,
                AddedServers = added,
                RemovedServers = removed
            });

            _logger?.LogInformation("已获取所有服务器数据  一共{Count}个  更新了{UnchangedCount}个  新增了{AddedCount}个  移除了{RemovedCount}个",
                newRowIdLst.Count, unchanged?.Count.ToString() ?? "N/A", added?.Count.ToString() ?? "N/A", removed?.Count.ToString() ?? "N/A");

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_dstConfig.ServerUpdateInterval ?? 60), HttpCancellationToken.Token);
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    private async Task UpdatingLoop()
    {
        if (_dstConfig.ServerDetailsUpdateInterval is null) return;

        await Task.Yield();
        await Task.Delay(20000); // 延迟,等待RequestLoop更新完

        Stopwatch s = new();
        DateTimeOffset lastUpdated = default;
        DateTimeOffset lastHistoryUpdateTime = new(); // 上次更新的详细信息历史记录时间
        while (Running)
        {
            //更新间隔
            if (DateTimeOffset.Now - lastUpdated < TimeSpan.FromSeconds(_dstConfig.ServerDetailsUpdateInterval.Value))
            {
                await Task.Delay(1000, HttpCancellationToken.Token);
                continue;
            }

            //ICollection<LobbyServerDetailed> arr = ServerMap.Values;
            var totalCount = ServerMap.Count;
            if (totalCount != 0)
            {
                s.Restart();
                try
                {
                    //历史记录更新间隔
                    bool isInsertHistory = false;
                    if (_dstConfig.HistoryDetailsUpdateInterval != null &&
                        DateTimeOffset.Now - lastHistoryUpdateTime >= TimeSpan.FromSeconds(_dstConfig.HistoryDetailsUpdateInterval.Value))
                    {
                        lastHistoryUpdateTime = DateTimeOffset.Now;
                        isInsertHistory = true;
                        _logger?.LogInformation("此次更新开始添加到历史记录 {DateTime}", lastHistoryUpdateTime);
                    }

                    //开始更新
                    var updatedCount = await LobbyDownloader.UpdateToDetails(ServerMap, updatedChunk =>
                    {
                        if (isInsertHistory)
                        {
                            DetailsChunkUpdated?.Invoke(this, new DstUpdatedDetailsChunk(updatedChunk, DateTimeOffset.Now));
                        }
                    }, HttpCancellationToken.Token);
                    //if (updatedCount > arr.Count * 0.6f) // 更新数量大于60%
                    //{
                    //    //Updated?.Invoke(this, new DstUpdatedEventArgs(updated, LastUpdate));
                    //}

                    _logger?.LogInformation("所有详细信息已更新  在{OriginCount}个中更新了{UpdateCount}  耗时:{ElapsedMilliseconds:0.00}分钟  距离上次更新{LateUpdate:0.00}分钟", totalCount, updatedCount, s.ElapsedMilliseconds / 1000 / 60.0, (DateTimeOffset.Now - lastUpdated).TotalMinutes);

                    s.Stop();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("服务器详细信息更新异常: {Exception}", ex.Message);
                }

                lastUpdated = DateTimeOffset.Now;
            }
        }
    }

    /// <summary>
    /// 获取当前当前所有房间的详细数据
    /// </summary>
    /// <returns></returns>
    public ICollection<LobbyServerDetailed> GetCurrentServers()
    {
        var servers = _serverCache;
        return servers;
    }

    /// <summary>
    /// 通过RowId获取房间详细信息
    /// </summary>
    /// <param name="rowid">RowId</param>
    /// <param name="forceUpdate">是否强制更新</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task<LobbyServerDetailed?> GetDetailedByRowIdAsync(string rowid, bool forceUpdate, CancellationToken cancellationToken)
    {
        var server = ServerMap.GetValueOrDefault(rowid);
        if (server is null) return null;

        lock (server)
        {
            server._Lock ??= new SemaphoreSlim(1);
        }
        var token = CancellationTokenSource.CreateLinkedTokenSource(HttpCancellationToken.Token, cancellationToken).Token;
        try
        {
            await server._Lock.WaitAsync(token);
            if (forceUpdate || !server.Raw!._IsDetailed || DateTimeOffset.Now - server.Raw._LastUpdate > TimeSpan.FromSeconds(20))
            {
                await LobbyDownloader.UpdateToDetails(server, HttpCancellationToken.Token);
            }
        }
        finally
        {
            server._Lock.Release();
        }
        return server;
    }

}


public class DstUpdatedEventArgs : EventArgs
{
    public ICollection<LobbyServerDetailed> Servers { get; init; }
    public ICollection<LobbyServer>? AddedServers { get; init; }
    public ICollection<LobbyServer>? RemovedServers { get; init; }
    public ICollection<LobbyServer>? UnchangedServers { get; init; }

    public DateTimeOffset UpdatedDateTime { get; init; }

    public DstUpdatedEventArgs(ICollection<LobbyServerDetailed> data, DateTimeOffset updatedDateTime)
    {
        Servers = data;
        UpdatedDateTime = updatedDateTime;
    }
}

public class DstUpdatedDetailsChunk(ICollection<LobbyServerDetailed> chunks, DateTimeOffset updatedDateTime) : EventArgs
{
    public ICollection<LobbyServerDetailed> Chunk { get; } = chunks;
    public DateTimeOffset UpdatedDateTime { get; } = updatedDateTime;
}
