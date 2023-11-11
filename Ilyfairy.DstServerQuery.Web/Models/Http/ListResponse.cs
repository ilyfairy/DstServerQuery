namespace Ilyfairy.DstServerQuery.Web.Models;

public class ListResponse<T>
{
    public DateTime DateTime { get; set; }
    public DateTime LastUpdate { get; set; }
    public int Count { get; set; }
    public int AllCount { get; set; }
    public int CurrentPageIndex { get; set; }
    public int MaxPageIndex { get; set; }
    public IEnumerable<T> List { get; set; } = Array.Empty<T>();
}
