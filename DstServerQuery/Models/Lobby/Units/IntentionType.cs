namespace DstServerQuery.Models.Lobby.Units;

/// <summary>
/// 游戏风格
/// </summary>

public record struct IntentionType
{
    public string? Value { get; set; }

    public IntentionType(string? value)
    {
        Value = value;
    }

    /// <summary>
    /// 轻松
    /// </summary>
    public static IntentionType Relaxed => new("relaxed");

    /// <summary>
    /// 无尽
    /// </summary>
    public static IntentionType Endless => new("endless");

    /// <summary>
    /// 生存
    /// </summary>
    public static IntentionType Survival => new("survival");

    /// <summary>
    /// 荒野
    /// </summary>
    public static IntentionType Wilderness => new("wilderness");

    /// <summary>
    /// 暗无天日
    /// </summary>
    public static IntentionType Lightsout => new("lightsout");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 合作
    /// </summary>
    public static IntentionType Cooperative => new("cooperative");

    /// <summary>
    /// 社交
    /// </summary>
    public static IntentionType Social => new("social");

    /// <summary>
    /// 疯狂
    /// </summary>
    public static IntentionType Madness => new("madness");

    /// <summary>
    /// 竞争
    /// </summary>
    public static IntentionType Competitive => new("competitive");

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//

    /// <summary>
    /// 海钓
    /// </summary>
    public static IntentionType OceanFishing => new("oceanfishing");
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