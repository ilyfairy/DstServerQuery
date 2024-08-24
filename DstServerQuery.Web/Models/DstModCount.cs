namespace DstServerQuery.Web.Models;

public class DstModCount(long id, string name, int count)
{
    public long Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Count { get; set; } = count;
}