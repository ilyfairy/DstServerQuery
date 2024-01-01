namespace Ilyfairy.DstServerQuery.Models;

/// <summary>
/// 大厅Mod信息
/// </summary>
public record LobbyModInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? CurrentVersion { get; set; }
    public string? NewVersion { get; set; }
    public bool IsClientDownload { get; set; }
}
