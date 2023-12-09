using Asp.Versioning;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Web.Models;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("fixed")]
public class ModsController : ControllerBase
{
    private readonly LobbyServerManager lobbyDetailsManager;
    private readonly ILogger _logger;

    public ModsController(ILogger<ModsController> logger, LobbyServerManager LobbyDetailsManager)
    {
        _logger = logger;
        lobbyDetailsManager = LobbyDetailsManager;
    }

    /// <summary>
    /// 获取Steam Mods的信息和使用量
    /// </summary>
    /// <returns></returns>
    [HttpPost("Usage")]
    public IActionResult GetModsUsage(int maxCount = 100)
    {
        //_logger.Info("获取Mod个数 TopCount{0}", maxCount);
        var list = lobbyDetailsManager.GetCurrentServers();

        IEnumerable<LobbyModInfo> mods = list.Where(v => v is { Platform: Platform.Steam, ModsInfo: { } })
            .SelectMany(v => v.ModsInfo ?? []);

        var group = mods.GroupBy(v => v.Id).Select(v =>
        {
            var mod = v.First();
            return new DstModCount(mod.Id, mod.Name, v.Count());
        });

        var r = group.OrderByDescending(v => v.Count);

        DstModsUsageResponse response = new();
        response.Mods = r.Take(maxCount);

        return response.ToJsonResult();
    }


    /// <summary>
    /// 获取Mod的名称和使用量
    /// </summary>
    /// <param name="min">最小个数</param>
    /// <returns></returns>
    [HttpPost("GetNames")]
    public IActionResult GetNames([FromQuery] int min = 1)
    {
        var list = lobbyDetailsManager.GetCurrentServers();

        IEnumerable<string> mods = list.SelectMany(v => v.ModsInfo ?? []).Select(v => v.Name);

        var group = mods.GroupBy(v => v).Select(v => new DstModNameCount(v.Key, v.Count()));

        var r = group.OrderByDescending(v => v.Count)
            .Where(v => v.Count >= min);

        DstModsNameUsageResponse response = new();
        response.Mods = r;

        return response.ToJsonResult();
    }

}
