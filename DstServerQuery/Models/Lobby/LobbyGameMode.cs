namespace DstServerQuery.Models.Lobby;

/// <summary>
/// 游戏模式
/// </summary>
public record struct LobbyGameMode
{
    public string? Value { get; set; }

    public LobbyGameMode(string? value)
    {
        Value = value;
    }

    /// <summary>
    /// 生存
    /// </summary>
    public static LobbyGameMode Survival => new("survival");

    /// <summary>
    /// 荒野
    /// </summary>
    public static LobbyGameMode Wilderness => new("wilderness");

    /// <summary>
    /// 无尽
    /// </summary>
    public static LobbyGameMode Endless => new("endless");

    /// <summary>
    /// 熔炉
    /// </summary>
    public static LobbyGameMode Lavaarena => new("lavaarena");

    /// <summary>
    /// 暴食
    /// </summary>
    public static LobbyGameMode Quagmire => new("quagmire");

    /// <summary>
    /// Starving Floor
    /// </summary>
    public static LobbyGameMode StarvingFloor => new("starving_floor");

    /// <summary>
    /// 格斗
    /// </summary>
    public static LobbyGameMode Smashup => new("smashup");
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
