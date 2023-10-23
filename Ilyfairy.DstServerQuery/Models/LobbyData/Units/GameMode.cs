namespace Ilyfairy.DstServerQuery.Models.LobbyData.Units;

/// <summary>
/// 游戏模式
/// </summary>
public record struct GameMode
{
    public string? Value { get; set; }

    public GameMode(string? value)
    {
        Value = value;
    }

    /// <summary>
    /// 生存
    /// </summary>
    public static GameMode Survival => new("survival");

    /// <summary>
    /// 荒野
    /// </summary>
    public static GameMode Wilderness => new("wilderness");

    /// <summary>
    /// 无尽
    /// </summary>
    public static GameMode Endless => new("endless");

    /// <summary>
    /// 熔炉
    /// </summary>
    public static GameMode Lavaarena => new("lavaarena");

    /// <summary>
    /// 暴食
    /// </summary>
    public static GameMode Quagmire => new("quagmire");

    /// <summary>
    /// Starving Floor
    /// </summary>
    public static GameMode StarvingFloor => new("starving_floor");

    /// <summary>
    /// 格斗
    /// </summary>
    public static GameMode Smashup => new("smashup");
}


///// <summary>
///// 游戏模式枚举
///// </summary>
//public enum GameMode
//{
//    /// <summary>
//    /// 未知
//    /// </summary>
//    unknown,
//    /// <summary>
//    /// 生存
//    /// </summary>
//    survival,
//    /// <summary>
//    /// 荒野
//    /// </summary>
//    wilderness,
//    /// <summary>
//    /// 无尽
//    /// </summary>
//    endless,
//    /// <summary>
//    /// 熔炉
//    /// </summary>
//    lavaarena,
//    /// <summary>
//    /// 暴食
//    /// </summary>
//    quagmire,
//    /// <summary>
//    /// Starving Floor
//    /// </summary>
//    starving_floor,
//    /// <summary>
//    /// 格斗
//    /// </summary>
//    smashup,
//}
