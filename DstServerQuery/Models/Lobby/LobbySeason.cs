namespace DstServerQuery.Models.Lobby;

/// <summary>
/// 季节
/// </summary>
public record struct LobbySeason(string? Value)
{
    /// <summary>
    /// 秋
    /// </summary>
    public static LobbySeason Autumn => new("autumn");
    /// <summary>
    /// 冬
    /// </summary>
    public static LobbySeason Winter => new("winter");
    /// <summary>
    /// 春
    /// </summary>
    public static LobbySeason Spring => new("spring");
    /// <summary>
    /// 夏
    /// </summary>
    public static LobbySeason Summer => new("summer");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 冬季或秋季
    /// </summary>
    public static LobbySeason AutumnOrSpring => new("autumnOrspring");
    /// <summary>
    /// 冬季或夏季
    /// </summary>
    public static LobbySeason WinterOrSummer => new("winterOrsummer");
    /// <summary>
    /// 随机
    /// </summary>
    public static LobbySeason AutumnOrWinterOrSpringOrSummer => new("autumnOrwinterOrspringOrsummer");
}



//public enum Season
//{
//    /// <summary>
//    /// 默认, 秋
//    /// </summary>
//    autumn,
//    /// <summary>
//    /// 冬
//    /// </summary>
//    winter,
//    /// <summary>
//    /// 春
//    /// </summary>
//    spring,
//    /// <summary>
//    /// 夏
//    /// </summary>
//    summer,
//    /// <summary>
//    /// 春或秋
//    /// </summary>
//    autumnOrspring,
//    /// <summary>
//    /// 冬季或夏季
//    /// </summary>
//    winterOrsummer,
//    /// <summary>
//    /// 随机
//    /// </summary>
//    autumnOrwinterOrspringOrsummer
//}
