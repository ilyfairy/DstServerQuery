using System.Diagnostics;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using System.Collections.Concurrent;
using Ilyfairy.DstServerQuery.Models.Requests;
using System.Collections.Immutable;
using Ilyfairy.DstServerQuery.LobbyJson;
using Serilog;

namespace Ilyfairy.DstServerQuery;

/// <summary>
/// 大厅的详细信息实时获取以及管理
/// </summary>
public class LobbyServerManager : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<LobbyServerManager>();
    public ConcurrentDictionary<string, LobbyServerDetailed> ServerMap { get; } = new(2, 40000);
    private ICollection<LobbyServerDetailed> serverCache = Array.Empty<LobbyServerDetailed>();
    public bool Running => !HttpCancellationToken.IsCancellationRequested;
    public LobbyDownloader LobbyDownloader { get; private set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdate { get; private set; }

    private readonly Stopwatch sw = Stopwatch.StartNew();
    private readonly DstWebConfig dstConfig;

    public CancellationTokenSource HttpCancellationToken { get; private set; } = new(0);

    public event DstDataUpdatedHandler? Updated;

    //参数是依赖注入
    public LobbyServerManager(DstWebConfig requestConfig, LobbyDownloader lobbyDownloader, DstJsonOptions dstJsonOptions) : base()
    {
        LobbyDownloader = lobbyDownloader;
        this.dstConfig = requestConfig;
    }


    #region 方法

    public async Task Start()
    {
        HttpCancellationToken = new();

        await LobbyDownloader.Initialize();

        _logger.Information("开始DownloadLoop Task");
        sw.Restart();

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
                _logger.Error("DownloadLoopException: {Exception}", e.Message);
            }
        });

        //循环更新详细数据
        _ = UpdatingLoop();
    }

    public void Dispose()
    {
        HttpCancellationToken.Cancel();
        GC.SuppressFinalize(this);
        _logger.Information("LobbyDetailsManager Dispose");
    }

    //循环获取数据  
    private async Task RequestLoop()
    {
        Dictionary<string, LobbyServerDetailed> tempServerRowIdMap = new(40000);
        while (Running)
        {
            try
            {
                tempServerRowIdMap.Clear();
                CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromMinutes(3));
                _logger.Information("开始 Download");

                int count = 0;
                await foreach (var item in LobbyDownloader.DownloadAllBriefs(CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpCancellationToken.Token).Token))
                {
                    tempServerRowIdMap[item.RowId] = item;
                    count++;
                }

                _logger.Information($"已获取所有服务器数据 一共{count}个");
            }
            catch (Exception e)
            {
                if (!Running || HttpCancellationToken.IsCancellationRequested)
                {
                    _logger.Warning("中断请求,结束");
                    break;
                }
                _logger.Warning($"请求失败,重新请求\n{e.Message}");
                continue;
            }

            if (!Running) break;

            LastUpdate = DateTimeOffset.Now;
            var currentRowIds = ServerMap.Keys;
            var newRowIds = tempServerRowIdMap.Keys;

            //重复的
            List<LobbyServer> unchanged = new(10000);
            foreach (var rowId in currentRowIds.Intersect(newRowIds))
            {
                var @new = tempServerRowIdMap[rowId];
                var server = ServerMap[rowId];
                (@new as LobbyServer).CopyTo(server);
                unchanged.Add(server);
            }

            //新增的
            List<LobbyServer> added = new(1000);
            foreach (var rowId in newRowIds.Except(currentRowIds))
            {
                var @new = tempServerRowIdMap[rowId];
                if (ServerMap.TryAdd(rowId, @new))
                {
                    added.Add(@new);
                }
            }

            //移除的
            List<LobbyServer> removed = new(1000);
            foreach (var rowId in currentRowIds.Except(newRowIds))
            {
                if (ServerMap.TryRemove(rowId, out var rm))
                {
                    removed.Add(rm);
                }
            }

            serverCache = ServerMap.Values;
            Updated?.Invoke(this, new DstUpdatedEventArgs(serverCache, false, DateTimeOffset.Now)
            {
                UnchangedServers = unchanged,
                AddedServers = added,
                RemovedServers = removed
            });

            try
            {
                await Task.Delay(20000, HttpCancellationToken.Token);
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    private async Task UpdatingLoop()
    {
        Stopwatch s = new();
        await Task.Yield();
        await Task.Delay(20000); // 延迟,等待RequestLoop更新完

        DateTimeOffset lastUpdated = default;
        while (Running)
        {
            if (DateTimeOffset.Now - lastUpdated < TimeSpan.FromSeconds(dstConfig.DetailsUpdateInterval ?? 600))
            {
                await Task.Delay(2000, HttpCancellationToken.Token);
                continue;
            }

            ICollection<LobbyServerDetailed> arr = ServerMap.Values;
            if (arr.Count != 0)
            {
                s.Restart();
                try
                {
                    var updated = await LobbyDownloader.UpdateToDetails(arr, HttpCancellationToken.Token);
                    if (updated.Count > arr.Count * 0.6f) // 更新数量大于60%
                    {
                        Updated?.Invoke(this, new DstUpdatedEventArgs(updated, true, LastUpdate));
                    }

                    _logger.Information("所有详细信息已更新  在{OriginCount}个中更新了{UpdateCount}  耗时:{ElapsedMilliseconds:0.00}分钟  距离上次更新{LateUpdate:0.00}分钟", arr.Count, updated.Count, s.ElapsedMilliseconds / 1000 / 60.0, (DateTimeOffset.Now - lastUpdated).TotalMinutes);

                    s.Stop();
                }
                catch(Exception ex)
                {
                    _logger.Warning("服务器详细信息更新异常: {Exception}", ex.Message);
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
        var servers = serverCache;
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
            if (forceUpdate || !server._IsDetails || (DateTimeOffset.Now - server._LastUpdate) > TimeSpan.FromSeconds(20))
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

    #endregion

}

public delegate void DstDataUpdatedHandler(object sender, DstUpdatedEventArgs e);

public class DstUpdatedEventArgs : EventArgs
{
    public ICollection<LobbyServerDetailed> Servers { get; init; }
    public ICollection<LobbyServer>? AddedServers { get; init; }
    public ICollection<LobbyServer>? RemovedServers { get; init; }
    public ICollection<LobbyServer>? UnchangedServers { get; init; }

    public bool IsDetailed { get; init; }
    public DateTimeOffset UpdatedDateTime { get; init; }

    public DstUpdatedEventArgs(ICollection<LobbyServerDetailed> data, bool isDetailed, DateTimeOffset updatedDateTime)
    {
        Servers = data;
        IsDetailed = isDetailed;
        UpdatedDateTime = updatedDateTime;
    }
}