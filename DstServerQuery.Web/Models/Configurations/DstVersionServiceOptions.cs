namespace DstServerQuery.Web.Models.Configurations;

public class DstVersionServiceOptions
{
    public bool IsEnabled { get; set; }
    public long? DefaultVersion { get; set; }
    public bool IsDisabledUpdate { get; set; }
}
