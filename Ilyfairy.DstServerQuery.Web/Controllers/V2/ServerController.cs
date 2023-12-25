using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.Helpers;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer;
using Ilyfairy.DstServerQuery.Web.Models;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

/// <summary>
/// 服务器控制器
/// </summary>
[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed")]
public class ServerController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly LobbyServerManager lobbyServerManager;
    private readonly DstVersionService dstVersionGetter;
    private readonly HistoryCountService historyCountManager;
    private readonly DstDbContext dbContext;

    public ServerController(
        ILogger<ServerController> logger,
        LobbyServerManager lobbyDetailsManager,
        DstVersionService dstVersionGetter,
        HistoryCountService historyCountManager,
        DstDbContext dbContext
        )
    {
        _logger = logger;
        this.lobbyServerManager = lobbyDetailsManager;
        this.dstVersionGetter = dstVersionGetter;
        this.historyCountManager = historyCountManager;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 获取服务器最新版本 返回文本
    /// </summary>
    /// <returns></returns>
    [HttpGet("Version")]
    [ProducesResponseType<string>(200)]
    [Produces("text/plain")]
    public IActionResult GetServerVersionGet()
    {
        return Ok(dstVersionGetter.Version?.ToString() ?? "null");
    }

    /// <summary>
    /// 获取服务器最新版本 返回Json
    /// </summary>
    /// <response code="200">获取成功</response>
    /// <response code="503">服务器未准备就绪</response>
    /// <returns></returns>
    [HttpPost("Version")]
    [Produces("application/json")]
    [ProducesResponseType<GetServerVersionResponse>(200)]
    [ProducesResponseType<GetServerVersionResponse>(503)]
    public IActionResult GetServerVersionPost()
    {
        return new GetServerVersionResponse(dstVersionGetter.Version).ToJsonResult();
    }


    /// <summary>
    /// 通过RowId获取服务器详细数据
    /// </summary>
    /// <param name="id">RowId</param>
    /// <param name="forceUpdate">是否强制刷新</param>
    /// <response code="200">获取成功</response>
    /// <response code="400">参数错误</response>
    /// <response code="404">找不到游戏服务器</response>
    /// <returns></returns>
    [HttpPost("Details/{id}")]
    [ProducesResponseType<ILobbyServerDetailedV2>(200)]
    [ProducesResponseType<ResponseBase>(404)]
    [ProducesResponseType<ResponseBase>(400)]
    [Produces("application/json")]
    public async Task<IActionResult> GetDetails(string id, [FromQuery] bool forceUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            //log.Warn("RowId为空");
            return ResponseBase.BadRequest();
        }

        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        LobbyServerDetailed? info = await lobbyServerManager.GetDetailedByRowIdAsync(id, forceUpdate, cts.Token);

        if (info == null)
        {
            _logger.LogWarning("找不到该服务器 RowId:{RowId}", id);
            return ResponseBase.NotFound(); //找不到该房间
        }
        _logger.LogInformation("找到服务器 RowId:{RowId} Name:{Name}", id, info.Name);

        DateTimeOffset lastUpdate = info.GetUpdateTime();
        ServerDetailsResponse response = new()
        {
            Server = info,
            LastUpdate = lastUpdate,
        };

        return response.ToJsonResult();
    }

    /// <summary>
    /// 通过RowId获取服务器详细数据
    /// </summary>
    /// <param name="id">RowId</param>
    /// <param name="forceUpdate">是否强制刷新</param>
    /// <response code="200">获取成功</response>
    /// <response code="400">参数错误</response>
    /// <response code="404">找不到游戏服务器</response>
    /// <returns></returns>
    [HttpPost("Details")]
    [ProducesResponseType<ILobbyServerDetailedV2>(200)]
    [ProducesResponseType<ResponseBase>(400)]
    [ProducesResponseType<ResponseBase>(404)]
    [Produces("application/json")]
    public Task<IActionResult> GetDetailsFromQuery([FromQuery] string id, [FromQuery] bool forceUpdate = false) => GetDetails(id, forceUpdate);


    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST
    ///     {
    ///         "ServerName":"Name", // 模糊匹配服务器名
    ///         "Season": {
    ///             "IsExclude": false,
    ///             "Value": "winter|summer" // 获取冬天和夏天的服务器
    ///         },
    ///         "Platform": "Steam", // 匹配Steam平台的服务器
    ///         "Version": "latest", // 匹配最新版本
    ///         "ip": "*.*.*.*", // 匹配任意IP地址
    ///         "PageIndex": 1, // 页索引
    ///         "Country": { // 排除为IP地址为null的国家
    ///             "IsExclude": true,
    ///             "Value": null
    ///         },
    ///         "ModsId": [362175979], // 匹配ModID
    ///         "IsDetailed": true, // 返回详细数据(包含玩家,Mods等信息)
    ///         "DaysInSeason": ">90%" // 这个季节已经过去了90%的天数(季节末期)
    ///         "Tags": { // 排除global和洞穴有关的Tags
    ///             "Value": ["global","洞穴", "caves"],
    ///             "IsExclude": true
    ///         },
    ///         "Connected": "&lt;100%", // 不是满人的服务器
    ///         "MaxConnections": ">10" // 最大人数大于10%
    ///         "PlayerPrefab": { // 过滤WX78和威尔逊这两个角色
    ///             "IsExclude": true, // 排除
    ///             "IsRegex": true, // 使用正则
    ///             "Value": "wx78|wilson" // 匹配WX78和威尔逊
    ///         }
    ///     }
    ///       
    /// </remarks>
    [HttpPost("List")]
    [ProducesResponseType<ListResponse<ILobbyServerDetailedV2>>(200)]
    [Produces("application/json")]
    public IActionResult GetServerList([FromBody] ListQueryParams? query = null)
    {
        var servers = lobbyServerManager.GetCurrentServers();

        query ??= new();

        LobbyServerQueryerV2 queryer = new(query, servers, dstVersionGetter.Version);

        ICollection<LobbyServerDetailed> result;
        try
        {
            result = queryer.Query();
        }
        catch (QueryArgumentException ex)
        {
            return ResponseBase.BadRequest(ex.Message);
        }

        int pageCount = query.PageCount ?? 100;
        int pageIndex = query.PageIndex ?? 0;

        if (pageCount > 1000)
            pageCount = 1000;
        if (pageCount < 1)
            pageCount = 1;

        if (pageIndex < 0)
            pageIndex = 0;

        var totalPageIndex = (int)Math.Ceiling((float)result.Count / pageCount) - 1;
        if (pageIndex > totalPageIndex)
            pageIndex = totalPageIndex;
        if (totalPageIndex < 0)
            totalPageIndex = 0;

        var current = result.Skip(pageCount * pageIndex).Take(pageCount).ToArray();

        ResponseBase CreateResponse<T>() where T : ILobbyServerV2
        {
            return new ListResponse<T>()
            {
                List = current.Cast<T>(),
                LastUpdate = lobbyServerManager.LastUpdate,
                TotalCount = result.Count,
                Count = current.Length,
                PageIndex = pageIndex,
                DateTime = DateTimeOffset.Now,
                MaxPageIndex = totalPageIndex,
            };
        }

        ResponseBase resonse;
        if (query.IsDetailed is true || query.PlayerName is not null || query.PlayerPrefab is not null)
        {
            resonse = CreateResponse<ILobbyServerDetailedV2>();
        }
        else
        {
            resonse = CreateResponse<ILobbyServerV2>();
        }

        return resonse.ToJsonResult();
    }

    /// <summary>
    /// 获取所有玩家预设
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetPrefabs")]
    [Produces("application/json")]
    [ProducesResponseType<PrefabsResponse>(200)]
    public IActionResult GetPrefabs()
    {
        var servers = lobbyServerManager.GetCurrentServers();

        var prefabs = servers.SelectMany(v => v.Players?.Select(player => player.Prefab) ?? [])
            .Where(v => !string.IsNullOrEmpty(v));

        var response = prefabs.GroupBy(v => v).Select(v => new PrefabsResponse.PlayerPrefab(v.Key, v.Count())).OrderByDescending(v => v.Count);
        
        return new PrefabsResponse(response).ToJsonResult();
    }

    /// <summary>
    /// 获取所有Tags
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetTags")]
    [Produces("application/json")]
    [ProducesResponseType<TagsResponse>(200)]
    public IActionResult GetTags([FromQuery] int min = 1)
    {
        var servers = lobbyServerManager.GetCurrentServers();

        var tags = servers.SelectMany(v => v.Tags ?? [])
            .Where(v => !string.IsNullOrEmpty(v));

        var response = tags.GroupBy(v => v)
            .Select(v => new TagsResponse.ServerTag(v.Key, v.Count()))
            .OrderByDescending(v => v.Count)
            .Where(v => v.Count >= min);

        return new TagsResponse(response).ToJsonResult();
    }

    /// <summary>
    /// 获取总计信息
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetTotal")]
    [Produces("application/json")]
    [ProducesResponseType<GetTotalResponse>(200)]
    public IActionResult GetTotal()
    {
        var servers = lobbyServerManager.GetCurrentServers();

        GetTotalResponse response = new();
        response.Version = dstVersionGetter.Version;
        response.Connections = servers.Sum(v => v.Connected);
        response.Servers = servers.Count;
        response.DateTime = DateTimeOffset.Now;

        return response.ToJsonResult();
    }
}
