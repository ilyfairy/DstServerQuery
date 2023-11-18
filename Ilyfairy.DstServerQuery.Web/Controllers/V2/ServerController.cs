using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.LobbyData.Interfaces;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer;
using Ilyfairy.DstServerQuery.Web.Models;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;
using System.Text.Json;

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
    private readonly HistoryCountManager historyCountManager;
    private readonly DstJsonOptions dstJsonOptions;
    private readonly DstDbContext dbContext;

    public ServerController(
        ILogger<ServerController> logger,
        LobbyServerManager lobbyDetailsManager,
        DstVersionService dstVersionGetter,
        HistoryCountManager historyCountManager,
        DstJsonOptions dstJsonOptions,
        DstDbContext dbContext
        )
    {
        _logger = logger;
        this.lobbyServerManager = lobbyDetailsManager;
        this.dstVersionGetter = dstVersionGetter;
        this.historyCountManager = historyCountManager;
        this.dstJsonOptions = dstJsonOptions;
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
    /// <returns></returns>
    [HttpPost("Version")]
    [Produces("application/json")]
    public IActionResult GetServerVersionPost()
    {
        return new DstResponse(new
        {
            Version = dstVersionGetter.Version,
        });
    }


    /// <summary>
    /// 通过RowId获取详细数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="forceUpdate">是否强制刷新</param>
    /// <returns></returns>
    [HttpPost("Details/{id}")]
    [ProducesResponseType<ILobbyServerDetailedV2>(200)]
    [Produces("application/json")]
    public async Task<IActionResult> GetDetails(string id, [FromQuery] bool forceUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            //log.Warn("RowId为空");
            return DstResponse.BadRequest();
        }

        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        LobbyServerDetailed? info = await lobbyServerManager.GetDetailedByRowIdAsync(id, forceUpdate, cts.Token);

        if (info == null)
        {
            _logger.LogWarning("找不到该服务器 RowId:{RowId}", id);
            return DstResponse.NotFound(); //找不到该房间
        }
        _logger.LogInformation("找到服务器 RowId:{RowId} Name:{Name}", id, info.Name);

        var json = JsonSerializer.Serialize<ILobbyServerDetailedV2>(info, dstJsonOptions.SerializerOptions);
        return Content(json, MediaTypeNames.Application.Json);
    }

    /// <summary>
    /// 获取服务器详细信息, 从query参数
    /// </summary>
    /// <param name="id">RowId</param>
    /// <param name="forceUpdate">是否强制刷新</param>
    /// <returns></returns>
    [HttpPost("Details")]
    [ProducesResponseType<ILobbyServerDetailedV2>(200)]
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
    public IActionResult GetServerList([FromBody] QueryParams? query = null)
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
            return DstResponse.BadRequest(ex.Message);
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

        ListResponse<T> CreateResponse<T>() where T : ILobbyServerV2
        {
            return new ListResponse<T>()
            {
                List = current.Cast<T>(),
                LastUpdate = lobbyServerManager.LastUpdate,
                AllCount = result.Count,
                Count = current.Length,
                PageIndex = pageIndex,
                DateTime = DateTime.Now,
                MaxPageIndex = totalPageIndex,
            };
        }

        object resonse;
        if (query.IsDetailed is true)
        {
            resonse = CreateResponse<ILobbyServerDetailedV2>();
        }
        else if (query.PlayerName is not null || query.PlayerPrefab is not null)
        {
            resonse = CreateResponse<ILobbyServerWithPlayerV2>();
        }
        else
        {
            resonse = CreateResponse<ILobbyServerV2>();
        }

        return new DstResponse(resonse)
        {
            SerializerSettings = dstJsonOptions.SerializerOptions,
        };
    }

    /// <summary>
    /// 获取所有玩家预设
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetPrefabs")]
    [Produces("application/json")]
    public IActionResult GetPrefabs()
    {
        var servers = lobbyServerManager.GetCurrentServers();

        var prefabs = servers.SelectMany(v => v.Players?.Select(player => player.Prefab) ?? [])
            .Where(v => !string.IsNullOrEmpty(v));

        var response = prefabs.GroupBy(v => v).Select(v => new
        {
            Prefab = v.Key,
            Count = v.Count()
        }).OrderByDescending(v => v.Count);

        return new DstResponse(response);
    }
}
