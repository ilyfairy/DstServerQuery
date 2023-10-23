using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ServerController : ControllerBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetLogger("ApiController");
    private readonly LobbyDetailsManager lobbyDetailsManager;
    private readonly DstVersionService dstVersionGetter;
    private readonly HistoryCountManager historyCountManager;
    private readonly DstJsonOptions dstJsonOptions;
    private readonly DstDbContext dbContext;

    public ServerController(
        LobbyDetailsManager lobbyDetailsManager,
        DstVersionService dstVersionGetter,
        HistoryCountManager historyCountManager,
        DstJsonOptions dstJsonOptions,
        DstDbContext dbContext
        )
    {
        this.lobbyDetailsManager = lobbyDetailsManager;
        this.dstVersionGetter = dstVersionGetter;
        this.historyCountManager = historyCountManager;
        this.dstJsonOptions = dstJsonOptions;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 获取服务器最新版本,文本
    /// </summary>
    /// <returns></returns>
    [HttpGet("version")]
    [HttpGet("v")]
    public IActionResult GetServerVersionGet()
    {
        return Ok(dstVersionGetter.Version?.ToString() ?? "null");
    }

    /// <summary>
    /// 获取服务器最新版本,Json
    /// </summary>
    /// <returns></returns>
    [HttpPost("version")]
    [HttpPost("v")]
    public IActionResult GetServerVersionPost()
    {
        return new JsonResult(new
        {
            Version = dstVersionGetter.Version,
        });
    }


    /// <summary>
    /// 通过RowId获取详细数据
    /// </summary>
    /// <param name="id"></param>   
    /// <returns></returns>
    [HttpPost("details/{id?}")]
    public async Task<IActionResult> GetDetails(string id, [FromQuery] bool forceUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            //log.Warn("RowId为空");
            return Content(@"{""msg"":""error""}", "application/json"); //参数为空
        }

        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        LobbyServerDetailed? info = await lobbyDetailsManager.GetDetailByRowIdAsync(id, forceUpdate, cts.Token);

        if (info == null)
        {
            _logger.Warn("找不到该服务器 RowId:{0}", id);
            return Content(@"{""msg"":""not found""}", "application/json"); //找不到该房间
        }
        _logger.Info("找到服务器 RowId:{0} Name:{1}", id, info.Name);
        return new JsonResult(info, dstJsonOptions.SerializerOptions);
    }

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<IActionResult> GetServerList()
    {
        var queryKey = Request.Query
            .Select(v => new KeyValuePair<string, string?>(v.Key, v.Value.FirstOrDefault()))
            .Where(v=>v.Value != null)
            .ToArray();

        //LobbyServerQueryerV1 queryer = new(lobbyDetailsManager.GetCurrentDetails(), queryKey, lobbyDetailsManager.LastUpdate);
        //queryer.Query();
        
        //string query = string.Join("&", queryKey.Select(v => $"{v.Key}={v.Value}"));
        //_logger.Info("查询服务器 Count:{0} Query:{1}", queryer.Result.Count, query);

        ListResponse<ILobbyServerWithPlayerV2> list = new();
        list.List = lobbyDetailsManager.GetCurrentDetails().Take(100);

        return new JsonResult(list, dstJsonOptions.SerializerOptions);
    }

    /// <summary>
    /// 获取服务器历史记录个数
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="rel"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpPost("historycount")]
    public async Task<IActionResult> GetServerCountHistory(
        [FromQuery] long startDateTime, 
        [FromQuery] long endDateTime, 
        [FromQuery] int interval = 60 * 60
        )
    {
        var start = DateTimeOffset.FromUnixTimeSeconds(startDateTime).DateTime;
        var end = DateTimeOffset.FromUnixTimeSeconds(endDateTime).DateTime;

        if (end - start > TimeSpan.FromDays(3) || start - end < default(TimeSpan))
        {
            return BadRequest();
        }

        DateTime currentDateTime = start;

        if (interval < 60) interval = 60;

        //linq表达式不能赋值
        //var linqResult = from item in dbContext.ServerHistoryCountInfos
        //                 where item.UpdateDate >= start && item.UpdateDate <= end
        //                 where item.UpdateDate >= currentDateTime && (currentDateTime = currentDateTime.AddSeconds(interval)) <= end
        //                 select item;

        //原生sql不会写
        //List<ServerCountInfo> usersInDb = dbContext.ServerHistoryCountInfos.FromSqlRaw
        //(
        //    """
        //    DECLARE @currentDateTime DATETIME
        //    SET @currentDateTime = @start;

        //    SELECT *
        //    FROM ServerHistoryCountInfos
        //    WHERE UpdateDate >= @start AND UpdateDate <= @end
        //    AND (
        //        UpdateDate >= @currentDateTime
        //        OR (@currentDateTime := DATEADD(SECOND, @interval, @currentDateTime)) IS NOT NULL
        //    )
        //    """,
        //    new SqlParameter("@start", start),
        //    new SqlParameter("@end", end),
        //    new SqlParameter("@interval", interval)
        //)
        //.ToList();


        ServerCountInfo[] r;
        if (start >= historyCountManager.First)
        {
            r = historyCountManager.Cache
                .Where(v => v.UpdateDate >= start && v.UpdateDate <= end)
                .ToArray();
        }
        else
        {
            r = await dbContext.ServerHistoryCountInfos
                .Where(v => v.UpdateDate >= start && v.UpdateDate <= end)
                .ToArrayAsync();
        }

        List<ServerCountInfo> result = new();

        ServerCountInfo? current = r.FirstOrDefault();
        foreach (var item in r)
        {
            if (item.UpdateDate >= currentDateTime)
            {
                current = item;
                currentDateTime = currentDateTime.AddSeconds(interval);
                result.Add(item);
            }
        }
        
        if(current != null && current != r[^1])
        {
            result.Add(r[^1]);
        }

        return new JsonResult(result);
    }
}
