namespace Ilyfairy.DstServerQuery.Web.Models;

public class DstModNameCount(string name, int count)
{
    public string Name { get; set; } = name;
    public int Count { get; set; } = count;
}
