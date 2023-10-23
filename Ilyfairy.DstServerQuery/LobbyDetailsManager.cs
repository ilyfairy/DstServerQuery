using System.Diagnostics;
using NLog;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using System.Collections.Concurrent;
using Ilyfairy.DstServerQuery.Models.Requests;
using System.Collections.Immutable;
using Ilyfairy.DstServerQuery.LobbyJson;

namespace Ilyfairy.DstServerQuery;

/// <summary>
/// 大厅的详细信息实时获取以及管理
/// </summary>
public class LobbyDetailsManager : IDisposable
{

    #region 属性字段
    public ConcurrentDictionary<string, LobbyServerDetailed> ServerMap { get; } = new(2, 40000);
    private ICollection<LobbyServerDetailed> serverCache = Array.Empty<LobbyServerDetailed>();
    public bool Running { get; private set; }
    public LobbyDownloader LobbyDownloader { get; private set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; private set; }

    private readonly Stopwatch sw = Stopwatch.StartNew();

    public CancellationTokenSource HttpTokenSource { get; private set; } = new();

    private bool oldUpdated;
    private bool newUpdated;

    private readonly object updateLock = new();
    #endregion

    #region Logger
    private readonly Logger logDownloadLoop = LogManager.GetLogger("QueryManager.DownloadLoop");
    #endregion

    public event DstDataUpdatedHandler? Updated;

    //参数是依赖注入
    public LobbyDetailsManager(RequestRoot rr, DstJsonOptions dstJsonOptions)
    {
        LobbyDownloader = new LobbyDownloader(dstJsonOptions, rr.Token, rr.DstDetailsProxyUrls);
    }

    #region 方法

    public async Task Start()
    {
        Running = true;
        await LobbyDownloader.Initialize();

        logDownloadLoop.Info("开始DownloadLoop Task");
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
                Running = false;
                Console.WriteLine($"NewDownloadLoopException: {e.Message}");
            }
        });

        //循环更新详细数据
        _ = UpdatingLoop();
    }

    public void Dispose()
    {
        logDownloadLoop.Info("LobbyDetailsManager Dispose");
        Running = false;
        HttpTokenSource.Cancel();
        GC.SuppressFinalize(this);
    }

    //循环获取数据  
    private async Task RequestLoop()
    {
        while (Running)
        {
            Dictionary<string, LobbyServerDetailed> data;
            try
            {
                // TODO: Download
                data = new();
                CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromMinutes(2));
                logDownloadLoop.Info("开始 Download");

                int count = 0;
                await foreach (var item in LobbyDownloader.DownloadAllBriefs(CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpTokenSource.Token).Token))
                {
                    data[item.RowId] = item;
                    count++;
                }

                logDownloadLoop.Info($"已获取所有服务器数据 一共{count}个");
            }
            catch (Exception e)
            {
                if (!Running)
                {
                    logDownloadLoop.Info("中断请求,结束");
                    break;
                }
                logDownloadLoop.Info($"请求失败,重新请求\n{e.Message}");
                continue;
            }

            if (!Running) break;

            var currentRowIds = ServerMap.Keys;
            var newRowIds = data.Select(v => v.Value.RowId).ToArray();

            //重复的
            foreach (var rowId in currentRowIds.Intersect(newRowIds))
            {
                (data[rowId] as LobbyServer).CopyTo(ServerMap[rowId]);
            }

            //新增的
            foreach (var rowId in newRowIds.Except(currentRowIds))
            {
                ServerMap.TryAdd(rowId, data[rowId]);
            }

            //移除的
            foreach (var rowId in currentRowIds.Except(newRowIds))
            {
                ServerMap.TryRemove(rowId, out _);
            }

            serverCache = ServerMap.Values;
            Updated?.Invoke(this, new DstUpdatedData(serverCache, false, DateTime.Now));

            lock (updateLock)
            {
                newUpdated = true;
            }

            UpdateEvent();

            try
            {
                await Task.Delay(20000, HttpTokenSource.Token);
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
        while (Running)
        {
            s.Restart();
            var arr = ServerMap.Values;
            if (arr.Count != 0)
            {
                try
                {
                    var count = await LobbyDownloader.UpdateToDetails(arr, HttpTokenSource.Token);
                    if (count <= 0 && arr.Count != 0)
                    {
                        logDownloadLoop.Info("@所有详细信息更新失败!");
                    }
                    else
                    {
                        logDownloadLoop.Info("@所有详细信息已更新!!!!!");
                    }
                }
                catch { }
            }
            await Task.Delay(20000, HttpTokenSource.Token);
            s.Stop();
            logDownloadLoop.Info($"更新详细信息耗时: {arr.Count}个 {s.ElapsedMilliseconds}ms");
        }
    }

    private void UpdateEvent()
    {
        lock (updateLock)
        {
            if (oldUpdated && newUpdated)
            {
                oldUpdated = false;
                newUpdated = false;
            }
            else
            {
                return;
            }
        }
        try
        {
            var list = GetCurrentDetails();
            LastUpdate = DateTime.Now;
            Updated?.Invoke(this, new DstUpdatedData(list, true, LastUpdate));

            logDownloadLoop.Info($"Details下载处理时间为: {sw.ElapsedMilliseconds}ms  房间个数为:{ServerMap.Count}");
            logDownloadLoop.Info($"记录更新: {LastUpdate}");
            logDownloadLoop.Info($"=======================");
            sw.Restart();
            Console.WriteLine();
            Console.WriteLine();
        }
        catch (Exception e)
        {
            File.AppendAllLines("error.txt", new[] { e.Message });
        }
    }

    /// <summary>
    /// 获取当前当前所有房间的详细数据
    /// </summary>
    /// <returns></returns>
    public ICollection<LobbyServerDetailed> GetCurrentDetails()
    {
        var servers = serverCache;
        return servers;
    }

    /// <summary>
    /// 通过RowId获取房间详细信息
    /// </summary>
    /// <param name="rowid"></param>
    /// <returns></returns>
    public async Task<LobbyServerDetailed?> GetDetailByRowIdAsync(string rowid, bool forceUpdate, CancellationToken cancellationToken)
    {
        var server = ServerMap.GetValueOrDefault(rowid);
        if (server is null) return null;
        //Interlocked.Exchange()
        lock (server)
        {
            server._Lock ??= new SemaphoreSlim(1);
        }
        var token = CancellationTokenSource.CreateLinkedTokenSource(HttpTokenSource.Token, cancellationToken).Token;
        try
        {
            await server._Lock.WaitAsync(token);
            if (forceUpdate || !server._IsDetails || (DateTime.Now - server._LastUpdate) > TimeSpan.FromSeconds(20))
            {
                await LobbyDownloader.UpdateToDetails(server, HttpTokenSource.Token);
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

public delegate void DstDataUpdatedHandler(object sender, DstUpdatedData e);

public class DstUpdatedData : EventArgs
{
    public ICollection<LobbyServerDetailed> Data { get; init; }
    public bool IsDetailed { get; init; }
    public DateTime UpdatedDateTime { get; init; }

    public DstUpdatedData(ICollection<LobbyServerDetailed> data, bool isDetailed, DateTime updatedDateTime)
    {
        Data = data;
        IsDetailed = isDetailed;
        UpdatedDateTime = updatedDateTime;
    }
}