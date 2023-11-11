namespace Ilyfairy.DstServerQuery.Web.Models;

/// <summary>
/// List响应结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListResponse<T>
{
    /// <summary>
    /// Http响应的时间
    /// </summary>
    public DateTime DateTime { get; set; }
    /// <summary>
    /// 数据最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; set; }
    /// <summary>
    /// 当前页个数
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// 所有个数
    /// </summary>
    public int AllCount { get; set; }
    /// <summary>
    /// 当前页索引
    /// </summary>
    public int CurrentPageIndex { get; set; }
    /// <summary>
    /// 最大页索引
    /// </summary>
    public int MaxPageIndex { get; set; }
    /// <summary>
    /// 服务器列表
    /// </summary>
    public IEnumerable<T> List { get; set; } = Array.Empty<T>();
}
