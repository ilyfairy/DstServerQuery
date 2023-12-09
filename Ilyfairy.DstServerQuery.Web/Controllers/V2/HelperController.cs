using Asp.Versioning;
using Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Web.Helpers;
using Ilyfairy.DstServerQuery.Web.Models.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("fixed")]
public class HelperController(
    ILogger<HelperController> _logger,
    DstDbContext _dbContext
    ) : ControllerBase
{
    /// <summary>
    /// 根据标签获取颜色
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    [HttpPost("GetTagsColor")]
    [Produces("application/json")]
    [ProducesResponseType<ColorResponse>(200)]
    public async Task<IActionResult> GetTagsColor([FromBody] string[] tags)
    {
        var colors = await _dbContext.TagColors.Where(v => tags.Contains(v.Name)).ToArrayAsync();

        Dictionary<string, string> colorDictionary = [];
        foreach (var color in colors)
        {
            colorDictionary.Add(color.Name, color.Name);
        }

        foreach (var item in tags.Except(colors.Select(v => v.Name)))
        {
            colorDictionary.Add(item, Helper.GetRandomColor(50, 180));
        }

        ColorResponse response = new()
        {
            Colors = colorDictionary
        };
        return response.ToJsonResult();
    }
}
