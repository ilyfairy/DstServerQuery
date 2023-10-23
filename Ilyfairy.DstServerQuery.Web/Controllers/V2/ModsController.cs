using Asp.Versioning;
using Ilyfairy.DstServerQuery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("[controller]/v{version:apiVersion}")]
public class ModsController : ControllerBase
{
    private readonly LobbyDetailsManager lobbyDetailsManager;
    private static readonly NLog.Logger _logger = NLog.LogManager.GetLogger("ModsController");

    public ModsController(LobbyDetailsManager LobbyDetailsManager)
    {
        lobbyDetailsManager = LobbyDetailsManager;
    }

    /// <summary>
    /// 获取Steam Mods个数
    /// </summary>
    /// <returns></returns>
    [HttpPost("usage")]
    public async Task<IActionResult> GetModsUsage(int maxCount = 100)
    {
        //_logger.Info("获取Mod个数 TopCount{0}", maxCount);
        var list = lobbyDetailsManager.GetCurrentDetails();

        IEnumerable<LobbyModInfo> mods = list.Where(v => v is { Platform: Platform.Steam, ModsInfo: { } })
            .SelectMany(v => v.ModsInfo);

        var group = mods.GroupBy(v => v.Id).Select(v =>
        {
            var mod = v.First();
            return new
            {
                Mod = new
                {
                    Id = mod.Id,
                    Name = mod.Name,
                },
                Count = v.Count(),
            };
        });

        var r = group.OrderByDescending(v => v.Count);

        return new JsonResult(r.Take(maxCount));
    }
}
