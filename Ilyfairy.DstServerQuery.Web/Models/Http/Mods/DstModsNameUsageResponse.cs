namespace Ilyfairy.DstServerQuery.Web.Models.Http.Mods;

public class DstModsNameUsageResponse : ResponseBase
{
    public IEnumerable<DstModNameCount> Mods { get; set; }

}
