namespace Ilyfairy.DstServerQuery.Web.Models.Configurations;

public class DstModsFileServiceOptions
{
    public bool IsEnabled { get; set; }
    public string RootPath { get; set; } = "mods";
    /// <summary>
    /// template: {url}
    /// </summary>
    public string? FileUrlProxy { get; set; }
}
