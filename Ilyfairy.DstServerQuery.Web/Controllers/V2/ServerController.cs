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
using System.Net.Mime;
using System.Text.Json;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
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
    /// 获取服务器最新版本,文本
    /// </summary>
    /// <returns></returns>
    [HttpGet("Version")]
    public IActionResult GetServerVersionGet()
    {
        return Ok(dstVersionGetter.Version?.ToString() ?? "null");
    }

    /// <summary>
    /// 获取服务器最新版本 返回Json
    /// </summary>
    /// <returns></returns>
    [HttpPost("Version")]
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
    public Task<IActionResult> GetDetailsFromQuery([FromQuery] string id, [FromQuery] bool forceUpdate = false) => GetDetails(id, forceUpdate);


    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("List")]
    public IActionResult GetServerList([FromBody] QueryParams query)
    {
        var servers = lobbyServerManager.GetCurrentServers();

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

        if (query.PageCount > 1000)
            query.PageCount = 1000;
        if(query.PageCount < 1)
            query.PageCount = 1;

        if (query.PageIndex < 0)
            query.PageIndex = 0;

        var totalPageIndex = (int)Math.Ceiling((float)result.Count / query.PageCount) - 1;
        if (query.PageIndex > totalPageIndex)
            query.PageIndex = totalPageIndex;
        if(totalPageIndex < 0)
            totalPageIndex = 0;

        var current = result.Skip(query.PageCount * query.PageIndex).Take(query.PageCount).ToArray();

        ListResponse<T> CreateResponse<T>() where T : ILobbyServerV2
        {
            return new ListResponse<T>()
            {
                List = current.Cast<T>(),
                LastUpdate = lobbyServerManager.LastUpdate,
                AllCount = result.Count,
                Count = current.Length,
                CurrentPageIndex = query.PageIndex,
                DateTime = DateTime.Now,
                MaxPageIndex = totalPageIndex,
            };
        }

        object resonse;
        if (query.IsDetailed)
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
