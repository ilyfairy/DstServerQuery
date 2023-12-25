using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Ilyfairy.DstServerQuery.Helpers;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web.Models;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("fixed")]
public class HistoryController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly LobbyServerManager lobbyDetailsManager;
    private readonly DstVersionService dstVersionGetter;
    private readonly HistoryCountService historyCountManager;
    private readonly DstDbContext dbContext;

    public HistoryController(
        ILogger<HistoryController> logger,
        LobbyServerManager lobbyDetailsManager,
        DstVersionService dstVersionGetter,
        HistoryCountService historyCountManager,
        DstDbContext dbContext
        )
    {
        _logger = logger;
        this.lobbyDetailsManager = lobbyDetailsManager;
        this.dstVersionGetter = dstVersionGetter;
        this.historyCountManager = historyCountManager;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 获取服务器历史记录个数
    /// </summary>
    /// <param name="interval">间隔</param>
    /// <param name="startDateTime">开始时间戳</param>
    /// <param name="endDateTime">结束时间戳</param>
    /// <returns></returns>
    [HttpPost("HistoryCount")]
    public async Task<IActionResult> GetServerCountHistory(
        [FromQuery] long startDateTime,
        [FromQuery] long endDateTime,
        [FromQuery] int interval = 60 * 60
        )
    {
        var start = DateTimeOffset.FromUnixTimeSeconds(startDateTime);
        var end = DateTimeOffset.FromUnixTimeSeconds(endDateTime);

        if (end - start > TimeSpan.FromDays(3) || start - end < default(TimeSpan))
        {
            return ResponseBase.BadRequest();
        }

        DateTimeOffset currentDateTime = start;

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

        var json = new ServerCountHistoryResponse()
        {
            List = result
        };

        return json.ToJsonResult();
    }

    /// <summary>
    /// 获取历史玩家
    /// </summary>
    /// <param name="name">玩家名</param>
    /// <param name="platform">平台</param>
    /// <param name="pageIndex">页索引</param>
    /// <returns></returns>
    [HttpPost("GetPlayers")]
    [ProducesResponseType<GetPlayersResponse>(200)]
    public async Task<IActionResult> GetPlayers([FromQuery] string? name = null, [FromQuery] Platform? platform = null, [FromQuery] int pageIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ResponseBase.BadRequest("'Name' is empty");
        }
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

        var pageCount = 100;

        var query = dbContext.Players.AsNoTracking().Where(playerExpression).Where(platformExpression);
        var allCount = await query.CountAsync();
        var maxPageIndex = (int)float.Ceiling((float)allCount / pageCount) - 1;

        if (pageIndex < 0)
            pageIndex = 0;
        if (pageIndex > maxPageIndex)
            pageIndex = maxPageIndex;

        var players = await query
            .OrderBy(v => v.Id)
            .Skip(pageIndex * pageCount)
            .Take(pageCount)
            .ToArrayAsync();

        GetPlayersResponse response = new()
        {
            List = players.Select(PlayerInfoItem.From).ToArray(),
            TotalCount = allCount,
            PageIndex = pageIndex,
            MaxPageIndex = maxPageIndex,
        };

        return response.ToJsonResult();
    }

    /// <summary>
    /// 获取玩家存在过的服务器
    /// </summary>
    /// <param name="playerId">玩家的NetId</param>
    /// <returns></returns>
    [HttpPost("GetPlayerServerHistory")]
    public async Task<IActionResult> GetPlayerServerHistory([FromQuery] string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return ResponseBase.BadRequest();
        }

        var servers = await dbContext.HistoryServerItemPlayerPair
            .AsNoTracking()
            .Where(v => v.PlayerId == playerId)
            .Select(v => v.HistoryServerItem.ServerId)
            .Distinct()
            .ToArrayAsync();

        PlayerServerHistoryResponse json = new()
        {
            Servers = servers,
        };

        return json.ToJsonResult();
    }

    /// <summary>
    /// 获取历史服务器的信息
    /// </summary>
    /// <param name="rowId">房间Id</param>
    /// <param name="startDateTime">开始时间</param>
    /// <param name="endDateTime">结束时间</param>
    /// <param name="isDetails">是否指定获取详细信息</param>
    /// <param name="includeDays">是否获取天数</param>
    /// <param name="includePlayers">是否获取玩家</param>
    /// <returns></returns>
    [HttpPost("GetServerHistory")]
    public async Task<IActionResult> GetServerHistory(
        [FromQuery] string rowId, 
        [FromQuery] DateTimeOffset? startDateTime = null, 
        [FromQuery] DateTimeOffset? endDateTime = null, 
        bool? isDetails = null,
        bool? includeDays = false,
        bool? includePlayers = false
        )
    {
        if (string.IsNullOrEmpty(rowId))
        {
            return ResponseBase.BadRequest();
        }

        var server = await dbContext.ServerHistories
            .FirstOrDefaultAsync(v => v.Id == rowId);

        if (server is null)
            return ResponseBase.NotFound();

        if (startDateTime is null || startDateTime < DateTimeOffset.Now - TimeSpan.FromDays(3))
            startDateTime = DateTimeOffset.Now - TimeSpan.FromDays(3);
        endDateTime ??= DateTimeOffset.Now;

        var query = dbContext.ServerHistoryItems
            .AsNoTracking()
            .Where(v => v.ServerId == server.Id);

        query = query.Where(v => v.DateTime >= startDateTime && v.DateTime <= endDateTime);

        if (isDetails == null)
        {
            query = query.Where(v => v.IsDetailed == (includeDays == true || includePlayers == true));
        }
        else
        {
            query = query.Where(v => v.IsDetailed == isDetails);
        }

        if(includeDays == true)
        {
            query = query.Include(v => v.DaysInfo);
        }
        if(includePlayers == true)
        {
            query = query.Include(v => v.Players);
        }

        var items = await query.OrderBy(v => v.DateTime).ToArrayAsync();
        ServerHistoryResponse response = new()
        {
            Server = server,
            Items = items.Select(v => ServerHistoryItem.From(v))
        };

        return response.ToJsonResult();
    }

}
