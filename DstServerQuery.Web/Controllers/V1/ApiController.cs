﻿using Asp.Versioning;
using DstServerQuery;
using DstServerQuery.EntityFrameworkCore;
using DstServerQuery.EntityFrameworkCore.Model.Entities;
using DstServerQuery.Models;
using DstServerQuery.Models.Lobby;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using DstServerQuery.Services;
using DstServerQuery.Web.Helpers.ServerQueryer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DstServerQuery.Web.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}")]
[Route("api")]
[EnableRateLimiting("fixed")]
public partial class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;

    private readonly DstVersionService _dstVersion;
    private readonly LobbyServerManager _lobbyServerManager;
    private readonly HistoryCountService _historyCountManager;
    private static readonly JsonSerializerOptions _v1JsonOptions = new();

    static ApiController()
    {
        _v1JsonOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
        };
        _v1JsonOptions.PropertyNamingPolicy = null;
        _v1JsonOptions.Converters.Add(new JsonStringEnumConverter());
        _v1JsonOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        _v1JsonOptions.MakeReadOnly();
    }

    //版本获取, 大厅服务器管理器, 大厅服务器历史房间数量管理器
    public ApiController(ILogger<ApiController> logger, DstVersionService versionGetter, LobbyServerManager lobbyDetailsManager, HistoryCountService historyCountManager)
    {
        _logger = logger;
        _dstVersion = versionGetter;
        this._lobbyServerManager = lobbyDetailsManager;
        this._historyCountManager = historyCountManager;
    }

    /// <summary>
    /// 获取服务器最新版本
    /// </summary>
    /// <returns></returns>
    [HttpPost("server/version")]
    [HttpPost("server/v")]
    [HttpGet("server/version")]
    [HttpGet("server/v")]
    public IActionResult GetServerVersion()
    {
        if (_dstVersion.Version == 0)
        {
            return Problem();
        }
        return new JsonResult(new
        {
            _dstVersion.Version,
        }, _v1JsonOptions);
    }

    /// <summary>
    /// 通过RowId获取详细数据
    /// </summary>
    /// <param name="id"></param>   
    /// <returns></returns>
    [HttpPost("details/{id}")]
    public async Task<IActionResult> GetDetails(string id, [FromQuery] bool forceUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("RowId为空");
            return Content(@"{""msg"":""error""}", "application/json"); //参数为空
        }

        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        LobbyServerDetailed? info = await _lobbyServerManager.GetDetailedByRowIdAsync(id, forceUpdate, cts.Token);

        if (info == null)
        {
            _logger.LogWarning("找不到该服务器 RowId:{RowId}", id);
            return Content(@"{""msg"":""not found""}", "application/json"); //找不到该房间
        }
        _logger.LogInformation("找到服务器 RowId:{RowId} Name:{Name}", id, info.Name);

        return Content(JsonSerializer.Serialize<ILobbyServerDetailedV1>(info, _v1JsonOptions), MediaTypeNames.Application.Json);
    }

    [HttpPost("details")]
    public Task<IActionResult> GetDetails2([FromQuery] string id, [FromQuery] bool forceUpdate = false) => GetDetails(id, forceUpdate);

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("list")]
    public IActionResult GetServerList()
    {
        var queryKey = Request.Query.Select(v => new KeyValuePair<string, string>(v.Key, v.Value.FirstOrDefault())).ToList();

        LobbyServerQueryerV1 queryer = new(_lobbyServerManager.GetCurrentServers(), queryKey, _lobbyServerManager.LastUpdate);
        queryer.Query();

        string query = string.Join("&", queryKey.Select(v => $"{v.Key}={v.Value}"));
        _logger.LogInformation("查询服务器 Count:{Count} Query:{Query}", queryer.Result.Count, query);

        return Content(queryer.ToJson(_v1JsonOptions), "application/json");
    }


    /// <summary>
    /// 获取服务器历史记录个数
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="rel"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpPost("server/historycount")]
    [HttpPost("serverhistorycount")]
    public IActionResult GetServerCountHistory(int interval = 3600, uint rel = 0, int count = 24)
    {
        _logger.LogInformation("获取服务器历史记录个数: Interval:{Interval} Rel:{Rel} Count:{Count}", interval, rel, count);
        if (interval <= 0) interval = 3600;
        DateTimeOffset date = DateTimeOffset.Now.AddSeconds(-rel);
        var history = _historyCountManager.GetServerHistory();

        List<ServerCountInfo> result = new();
        long last = 0;
        for (int i = history.Length; i-- > 0;)
        {
            var item = history[i];
            try
            {
                if (item.UpdateDate < date - TimeSpan.FromSeconds(last))
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
        return new JsonResult(result);
    }

    /// <summary>
    /// 获取mod个数
    /// </summary>
    /// <param name="topcount"></param>
    /// <returns></returns>
    [HttpPost("modscount")]
    public async Task<IActionResult> GetModsCount(int topcount = 100)
    {
        _logger.LogInformation("获取Mod个数 TopCount:{TopCount}", topcount);
        var list = _lobbyServerManager.GetCurrentServers();
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
        return new JsonResult(result.Take(topcount), _v1JsonOptions);
    }

    /// <summary>
    /// Mod个数信息
    /// </summary>
    public class ModCountInfo
    {
        public required string Name { get; set; }
        public int Count { get; set; }
        public long ID { get; set; }
    }
}
