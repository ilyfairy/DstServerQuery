using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Net.Mime;
using System.Text.Json;

namespace DstToolsWeb.Controllers;

public partial class ApiController : Controller
{
    private static readonly Logger log = LogManager.GetLogger("Web.Api.List");

    private readonly DstVersionGetter dstVersion;
    private readonly LobbyDetailsManager lobbyDetailsManager;
    private readonly HistoryCountManager historyCountManager;
    private readonly DstJsonOptions dstJsonOptions;

    //版本获取, 大厅服务器管理器, 大厅服务器历史房间数量管理器
    public ApiController(DstVersionGetter versionGetter, LobbyDetailsManager lobbyDetailsManager, HistoryCountManager historyCountManager, DstJsonOptions dstJsonOptions)
    {
        dstVersion = versionGetter;
        this.lobbyDetailsManager = lobbyDetailsManager;
        this.historyCountManager = historyCountManager;
        this.dstJsonOptions = dstJsonOptions;
    }

    /// <summary>
    /// 获取服务器最新版本
    /// </summary>
    /// <returns></returns>
    [HttpPost("api/server/version")]
    [HttpPost("api/server/v")]
    [HttpGet("api/server/version")]
    [HttpGet("api/server/v")]
    public IActionResult GetServerVersion()
    {
        if (dstVersion.Version == 0)
        {
            return this.Problem();
        }
        return Json(new
        {
            Version = dstVersion.Version,
        });
    }

    /// <summary>
    /// 通过RowId获取详细数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("api/details/{id?}")]
    public async Task<IActionResult> GetDetails(string id, [FromQuery] bool forceUpdate = false)
    {
        var log = LogManager.GetLogger("Web.Api.Details");
        if (string.IsNullOrWhiteSpace(id))
        {
            log.Warn("RowId为空");
            return Content(@"{""msg"":""error""}", "application/json"); //参数为空
        }

        CancellationTokenSource cts = new();
        cts.CancelAfter(10000);
        LobbyDetailsData? info = await lobbyDetailsManager.GetDetailByRowIdAsync(id, forceUpdate, cts.Token);

        if (info == null)
        {
            log.Warn("找不到该服务器 RowId:{0}", id);
            return Content(@"{""msg"":""not found""}", "application/json"); //找不到该房间
        }
        log.Info("找到服务器 RowId:{0} Name:{1}", id, info.Name);
        return Content(JsonSerializer.Serialize(info, dstJsonOptions.SerializerOptions), MediaTypeNames.Application.Json);
    }

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("api/list")]
    public async Task<IActionResult> GetServerList()
    {
        var queryKey = Request.Query.Select(v => new KeyValuePair<string, string>(v.Key, v.Value.FirstOrDefault())).ToList();

        LobbyServerQueryer queryer = new(lobbyDetailsManager.GetCurrentDetails(), queryKey, lobbyDetailsManager.LastUpdate);
        queryer.Query();

        string query = string.Join("&", queryKey.Select(v => $"{v.Key}={v.Value}"));
        log.Info("查询服务器 Count:{0} Query:{1}", queryer.Result.Count, query);

        return Content(queryer.ToJson(dstJsonOptions.SerializerOptions), "application/json");
    }


    /// <summary>
    /// 获取服务器历史记录个数
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="rel"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpPost("api/server/historycount")]
    [HttpPost("api/serverhistorycount")]
    public IActionResult GetServerCountHistory(int interval = 3600, uint rel = 0, int count = 24)
    {
        var log = LogManager.GetLogger("Web.Api.GetServerCountHistory");
        log.Info("获取服务器历史记录个数: Interval:{0} Rel:{1} Count:{2}", interval, rel, count);
        if (interval <= 0) interval = 3600;
        DateTime date = DateTime.Now.AddSeconds(-rel);
        var history = historyCountManager.GetServerHistory();

        List<ServerCountInfo> result = new();
        long last = 0;
        for (int i = history.Length; i-- > 0;)
        {
            var item = history[i];
            try
            {
                if (new DateTimeOffset(item.UpdateDate).ToUnixTimeSeconds() < (new DateTimeOffset(date).ToUnixTimeSeconds() - last))
                {
                    result.Add(item);
                    if (result.Count >= count)
                    {
                        break;
                    }
                    last += interval;
                }
            }
            catch { }
        }
        result.Reverse();
        return Json(result, dstJsonOptions.SerializerOptions);
    }

    /// <summary>
    /// 获取mod个数
    /// </summary>
    /// <param name="topcount"></param>
    /// <returns></returns>
    [HttpPost("api/modscount")]
    public async Task<IActionResult> GetModsCount(int topcount = 100)
    {
        var log = LogManager.GetLogger("Web.Api.GetModsCount");
        log.Info("获取Mod个数 TopCount{0}", topcount);
        var list = lobbyDetailsManager.GetCurrentDetails();
        var key = new Dictionary<long, ModCountInfo>();

        foreach (var item in list)
        {
            if (item.ModsInfo is null || item.Platform != Platform.Steam) continue;
            foreach (var mod in item.ModsInfo)
            {
                if (mod is null) continue;
                if (key.TryGetValue(mod.Id, out var n))
                {
                    key[mod.Id].Count++;
                }
                else
                {
                    key[mod.Id] = new ModCountInfo()
                    {
                        Name = mod.Name,
                        Count = 1,
                        ID = mod.Id,
                    };
                }
            }
        }
        var result = key.Values.OrderByDescending(v => v.Count);
        return Json(result.Take(topcount));
    }

    /// <summary>
    /// Mod个数信息
    /// </summary>
    public class ModCountInfo
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public long ID { get; set; }
    }
}
