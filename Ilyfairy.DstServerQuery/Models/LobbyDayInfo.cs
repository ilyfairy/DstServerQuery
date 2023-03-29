namespace Ilyfairy.DstServerQuery.Models;

public class LobbyDayInfo
{
    /// <summary>
    /// 当前天数
    /// </summary>
    public int Day { get; set; }
    /// <summary>
    /// 当前季节已过去天数
    /// </summary>

    public int DaysElapsedInSeason { get; set; }
    /// <summary>
    /// 当前季节剩余天数
    /// </summary>
    public int DaysLeftInSeason { get; set; }
}
