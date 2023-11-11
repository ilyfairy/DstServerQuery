using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly LobbyServerManager lobbyDetailsManager;
    private readonly DstVersionService dstVersionGetter;
    private readonly HistoryCountManager historyCountManager;
    private readonly DstJsonOptions dstJsonOptions;
    private readonly DstDbContext dbContext;

    public HistoryController(
        ILogger<HistoryController> logger,
        LobbyServerManager lobbyDetailsManager,
        DstVersionService dstVersionGetter,
        HistoryCountManager historyCountManager,
        DstJsonOptions dstJsonOptions,
        DstDbContext dbContext
        )
    {
        _logger = logger;
        this.lobbyDetailsManager = lobbyDetailsManager;
        this.dstVersionGetter = dstVersionGetter;
        this.historyCountManager = historyCountManager;
        this.dstJsonOptions = dstJsonOptions;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 获取服务器历史记录个数
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="startDateTime">开始时间</param>
    /// <param name="endDateTime">结束时间</param>
    /// <returns></returns>
    [HttpPost("HistoryCount")]
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
            return DstResponse.BadRequest();
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
            r = await dbContext.ServerHistoryCountInfos.AsNoTracking()
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

        if (current != null && current != r[^1])
        {
            result.Add(r[^1]);
        }

        var json = new ServerCountHistory()
        {
            List = result
        };

        return new DstResponse(json);
    }

    /// <summary>
    /// 获取所有历史玩家
    /// </summary>
    /// <param name="name">玩家名</param>
    /// <param name="platform">平台</param>
    /// <returns></returns>
    [HttpPost("GetPlayers")]
    public async Task<IActionResult> GetPlayers([FromQuery] string? name = null, [FromQuery] Platform? platform = null)
    {
        Expression<Func<DstPlayer, bool>> playerExpression;
        if (name == null)
        {
            playerExpression = player => true;
        }
        else
        {
            playerExpression = player => player.Name.Contains(name);
        }

        Expression<Func<DstPlayer, bool>> platformExpression;
        if (platform == null)
        {
            platformExpression = player => true;
        }
        else
        {
            platformExpression = player => player.Platform == platform;
        }

        var players = await dbContext.Players.AsNoTracking().Where(playerExpression).Where(platformExpression).ToArrayAsync();

        return new JsonResult(players.Select(v => new
        {
            Id = v.Id,
            Name = v.Name,
            Platform = v.Platform.ToString(),
        }));
    }

    /// <summary>
    /// 获取玩家存在于哪个服务器中
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    [HttpPost("GetPlayerServerHistory")]
    public async Task<IActionResult> GetPlayerServerHistory([FromQuery] string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return DstResponse.BadRequest();
        }

        var servers = await dbContext.HistoryServerItemPlayerPair
            .AsNoTracking()
            .Where(v => v.PlayerId == playerId)
            .Select(v => v.HistoryServerItem.ServerId)
            .ToArrayAsync();

        PlayerServerHistoryResponse json = new()
        {
            Servers = servers,
        };

        return new DstResponse(json);
    }

    /// <summary>
    /// 获取历史服务器的信息
    /// </summary>
    /// <param name="rowId">房间Id</param>
    /// <param name="isDetailed">是否详细信息</param>
    /// <param name="startDateTime">开始时间</param>
    /// <param name="endDateTime">结束时间</param>
    /// <returns></returns>
    [HttpPost("GetServerHistory")]
    public async Task<IActionResult> GetServerHistory([FromQuery] string rowId, [FromQuery] bool isDetailed = false, [FromQuery] DateTime? startDateTime = null, [FromQuery] DateTime? endDateTime = null)
    {
        if (string.IsNullOrEmpty(rowId))
        {
            return DstResponse.BadRequest();
        }

        var server = await dbContext.ServerHistories
            .FirstOrDefaultAsync(v => v.Id == rowId);

        if (server is null)
            return DstResponse.NotFound();

        startDateTime ??= default;
        endDateTime ??= DateTime.Now;

        var items = await dbContext.ServerHistoryItems
            .AsNoTracking()
            .Include(v => v.DaysInfo)
            .Include(v => v.Players)
            .Where(v => v.DateTime >= startDateTime && v.DateTime <= endDateTime)
            .Where(v => v.ServerId == server.Id)
            .Where(v => v.IsDetailed == isDetailed)
            .ToArrayAsync();

        ServerHistoryResponse json = new()
        {
            Server = server,
            Items = items
        };

        return new DstResponse(json);
    }

}
