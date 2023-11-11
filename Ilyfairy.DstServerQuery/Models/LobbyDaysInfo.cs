namespace Ilyfairy.DstServerQuery.Models;

public record class LobbyDaysInfo
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

    /// <summary>
    /// 当前季节总天数
    /// </summary>
    public int TotalDaysSeason => DaysElapsedInSeason + DaysLeftInSeason;
}
