using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using Ilyfairy.DstServerQuery.Models;

namespace Ilyfairy.DstServerQuery.Web.Models;

/// <summary>
/// 玩家信息
/// </summary>
public class PlayerInfoItem
{
    /// <summary>
    /// 玩家Id
    /// </summary>
    public required string NetId { get; set; }

    /// <summary>
    /// 玩家名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 所在的平台
    /// </summary>
    public required Platform Platform { get; set; }

    public static PlayerInfoItem From(DstPlayer player)
    {
        return new()
        {
            NetId = player.Id,
            Name = player.Name,
            Platform = player.Platform,
        };
    }
}
