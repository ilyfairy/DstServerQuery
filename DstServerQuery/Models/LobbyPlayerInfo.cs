namespace DstServerQuery.Models;

/// <summary>
/// 大厅玩家信息
/// </summary>
public class LobbyPlayerInfo
{
    /// <summary>
    /// 玩家名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 游戏内颜色
    /// </summary>
    public string Color { get; set; }
    public int EventLevel { get; set; }
    public string NetId { get; set; }
    /// <summary>
    /// 角色
    /// </summary>
    public string Prefab { get; set; }
}
