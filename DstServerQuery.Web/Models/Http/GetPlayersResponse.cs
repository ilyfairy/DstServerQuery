using DstServerQuery.Web.Models;

namespace DstServerQuery.Web.Models.Http;

public class GetPlayersResponse : ResponseBase
{
    /// <summary>
    /// 玩家列表
    /// </summary>
    public required ICollection<PlayerInfoItem> List { get; set; }

    /// <summary>
    /// 所有个数
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// 当前页个数
    /// </summary>
    public int Count => List.Count;

    /// <summary>
    /// 页索引
    /// </summary>
    public required int PageIndex { get; set; }

    /// <summary>
    /// 最大页索引
    /// </summary>
    public required int MaxPageIndex { get; set; }
}
