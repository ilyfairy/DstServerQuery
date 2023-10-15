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
    public ConcurrentDictionary<string, LobbyDetailsData> ServerMap { get; private set; } = new(2, 40000); //会被更新
    public Dictionary<string, LobbyDetailsData> OldServerMap { get; private set; } = new(); //会被替换
    private ICollection<LobbyDetailsData> ServerMapCache = Array.Empty<LobbyDetailsData>();
    public bool Running { get; private set; }
    public LobbyDownloader LobbyDownloader { get; private set; }
    public string[] OldUrlList { get; private set; }

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
    private readonly DstJsonOptions dstJsonOptions;
    #endregion

    public event DstDataUpdatedHandler? Updated;

    //参数是依赖注入
    public LobbyDetailsManager(RequestRoot rr, DstJsonOptions dstJsonOptions)
    {
        OldUrlList = rr.OldList;
        LobbyDownloader = new LobbyDownloader(dstJsonOptions, rr.Token, rr.DstDetailsProxyUrls);
        this.dstJsonOptions = dstJsonOptions;
    }

    #region 方法

    public async Task Start()
    {
        Running = true;
        ServerMap = new();
        await LobbyDownloader.Initialize();

        logDownloadLoop.Info("开始DownloadLoop Task");
        sw.Restart();

        //循环获取旧版服务器数据
        _ = Task.Run(async () =>
        {
            try
            {
                await OldRequestLoop();
            }
            catch (Exception e)
            {
                Running = false;
                Console.WriteLine($"OldDownloadLoopException: {e.Message}");
            }
        });

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
            Dictionary<string, LobbyDetailsData> data;
            try
            {
                // TODO: Download
                data = new();
                CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromMinutes(2));
                logDownloadLoop.Info("开始 Download");
                var r = await LobbyDownloader.DownloadAllBriefs(CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpTokenSource.Token).Token);

                logDownloadLoop.Info("已获取所有Briefs数据");
                foreach (var item in r)
                {
                    data[item.RowId] = item;
                }
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
                (data[rowId] as LobbyBriefsData).CopyTo(ServerMap[rowId]);
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

            ServerMapCache = ServerMap.Values;

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

    //旧接口服务器更新
    private async Task OldRequestLoop()
    {
        while (Running)
        {
            ConcurrentBag<List<LobbyDetailsData>> bag = new();
            try
            {
                await Parallel.ForEachAsync(OldUrlList, async (url, t) =>
                {
                    try
                    {
                        var r = await LobbyDownloader.DownloadDetails(url, HttpTokenSource.Token);
                        bag.Add(r);
                    }
                    catch { }
                });
            }
            catch (Exception e)
            {
                if (!Running) break;
            }
            Dictionary<string, LobbyDetailsData> oldMap = new(bag.Sum(v => v.Count));
            foreach (var region in bag)
            {
                foreach (var item in region)
                {
                    oldMap[item.RowId] = item;
                }
            }
            OldServerMap = oldMap;
            lock (updateLock)
            {
                oldUpdated = true;
            }
            logDownloadLoop.Info("所有旧接口信息已获取!");
            UpdateEvent();
            try
            {
                await Task.Delay(20000, HttpTokenSource.Token);
            }
            catch (Exception)
            {
                if (!Running) break;
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
            if (arr.Count == 0)
            {
                await Task.Delay(20000, HttpTokenSource.Token);
            }
            try
            {
                var count = await LobbyDownloader.UpdateToDetails(arr.ToArray(), HttpTokenSource.Token);
                if (count <= 0 && arr.Count != 0)
                {
                    logDownloadLoop.Info("@所有详细信息更新失败!");
                }
                else
                {
                    logDownloadLoop.Info("@所有详细信息已更新!!!!!");
                }
                await Task.Delay(20000, HttpTokenSource.Token);
            }
            catch { }
            s.Stop();
            logDownloadLoop.Info($"更新详细信息耗时: {s.ElapsedMilliseconds}ms");
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
    public ICollection<LobbyDetailsData> GetCurrentDetails()
    {
        var newData = ServerMapCache;
        var oldData = OldServerMap.Values;
        List<LobbyDetailsData> list = new(newData.Count + oldData.Count);
        list.AddRange(newData);
        list.AddRange(oldData);
        return list;
    }

    /// <summary>
    /// 通过RowId获取房间详细信息
    /// </summary>
    /// <param name="rowid"></param>
    /// <returns></returns>
    public async Task<LobbyDetailsData?> GetDetailByRowIdAsync(string rowid, bool forceUpdate, CancellationToken cancellationToken)
    {
        if (OldServerMap.TryGetValue(rowid, out var oldInfo))
        {
            return oldInfo;
        }
        var newInfo = ServerMap.GetValueOrDefault(rowid);
        if (newInfo is null) return null;
        //Interlocked.Exchange()
        lock (newInfo)
        {
            newInfo._Lock ??= new SemaphoreSlim(0, 1);
        }
        try
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(HttpTokenSource.Token, cancellationToken).Token;
            await newInfo._Lock.WaitAsync(token);
            if (forceUpdate || !newInfo._IsDetails || (DateTime.Now - newInfo._LastUpdate) > TimeSpan.FromSeconds(20))
            {
                await LobbyDownloader.UpdateToDetails(newInfo, HttpTokenSource.Token);
            }
        }
        finally
        {
            newInfo._Lock.Release();
        }
        return newInfo;
    }

    #endregion

}

public delegate void DstDataUpdatedHandler(object sender, DstUpdatedData e);

public class DstUpdatedData : EventArgs
{
    public ICollection<LobbyDetailsData> Data { get; init; }
    public bool IsDownloadCompleted { get; init; }
    public DateTime UpdatedDateTime { get; init; }

    public DstUpdatedData(ICollection<LobbyDetailsData> data, bool isDownloadCompleted, DateTime updatedDateTime)
    {
        Data = data;
        IsDownloadCompleted = isDownloadCompleted;
        UpdatedDateTime = updatedDateTime;
    }
}