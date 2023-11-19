namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class DstModsNameUsageResponse : ResponseBase
{
    public IEnumerable<DstModNameCount> Mods { get; set; }

}
