namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class DstModsUsageResponse : ResponseBase
{
    public IEnumerable<DstModCount> Mods { get; set; }
}