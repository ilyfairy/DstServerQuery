namespace DstServerQuery.Models.Lobby.Units;

/// <summary>
/// 季节
/// </summary>
public record struct Season(string? Value)
{
    /// <summary>
    /// 秋
    /// </summary>
    public static Season Autumn => new("autumn");
    /// <summary>
    /// 冬
    /// </summary>
    public static Season Winter => new("winter");
    /// <summary>
    /// 春
    /// </summary>
    public static Season Spring => new("spring");
    /// <summary>
    /// 夏
    /// </summary>
    public static Season Summer => new("summer");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 冬季或秋季
    /// </summary>
    public static Season AutumnOrSpring => new("autumnOrspring");
    /// <summary>
    /// 冬季或夏季
    /// </summary>
    public static Season WinterOrSummer => new("winterOrsummer");
    /// <summary>
    /// 随机
    /// </summary>
    public static Season AutumnOrWinterOrSpringOrSummer => new("autumnOrwinterOrspringOrsummer");
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
