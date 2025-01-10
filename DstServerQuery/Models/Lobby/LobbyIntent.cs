namespace DstServerQuery.Models.Lobby;

/// <summary>
/// 游戏风格
/// </summary>

public record struct LobbyIntent
{
    public string? Value { get; set; }

    public LobbyIntent(string? value)
    {
        Value = value;
    }

    /// <summary>
    /// 轻松
    /// </summary>
    public static LobbyIntent Relaxed => new("relaxed");

    /// <summary>
    /// 无尽
    /// </summary>
    public static LobbyIntent Endless => new("endless");

    /// <summary>
    /// 生存
    /// </summary>
    public static LobbyIntent Survival => new("survival");

    /// <summary>
    /// 荒野
    /// </summary>
    public static LobbyIntent Wilderness => new("wilderness");

    /// <summary>
    /// 暗无天日
    /// </summary>
    public static LobbyIntent Lightsout => new("lightsout");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 合作
    /// </summary>
    public static LobbyIntent Cooperative => new("cooperative");

    /// <summary>
    /// 社交
    /// </summary>
    public static LobbyIntent Social => new("social");

    /// <summary>
    /// 疯狂
    /// </summary>
    public static LobbyIntent Madness => new("madness");

    /// <summary>
    /// 竞争
    /// </summary>
    public static LobbyIntent Competitive => new("competitive");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 海钓
    /// </summary>
    public static LobbyIntent OceanFishing => new("oceanfishing");
}


//public enum IntentionType
//{
//    /// <summary>
//    /// 未知
//    /// </summary>
//    unknown,

//    /// <summary>
//    /// 轻松
//    /// </summary>
//    relaxed,
//    /// <summary>
//    /// 无尽
//    /// </summary>
//    endless,
//    /// <summary>
//    /// 生存
//    /// </summary>
//    survival,
//    /// <summary>
//    /// 荒野
//    /// </summary>
//    wilderness,
//    /// <summary>
//    /// 暗无天日
//    /// </summary>
//    lightsout,

//    /// <summary>
//    /// 合作
//    /// </summary>
//    cooperative,
//    /// <summary>
//    /// 合作
//    /// </summary>
//    ooperative,
//    /// <summary>
//    /// 社交
//    /// </summary>
//    social,
//    /// <summary>
//    /// 疯狂
//    /// </summary>
//    madness,
//    /// <summary>
//    /// 竞争
//    /// </summary>
//    competitive,

//    /// <summary>
//    /// 海钓
//    /// </summary>
//    oceanfishing,
//}