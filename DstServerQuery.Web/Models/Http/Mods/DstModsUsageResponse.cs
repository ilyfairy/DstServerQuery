namespace DstServerQuery.Web.Models.Http.Mods;

public class DstModsUsageResponse : ResponseBase
{
    public IEnumerable<DstModCount> Mods { get; set; }
}