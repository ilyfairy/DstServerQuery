namespace Ilyfairy.DstServerQuery.Models;

/// <summary>
/// 平台
/// </summary>
public enum Platform
{
    None = 0x00,
    Steam = 0x01, // 前缀R
    PlayStation = 0x02, // 前缀P开头
    WeGame = 0x04,
    QQGame = 0x08,
    Xbox = 0x10,
    Switch = 0x20,

    PS4Official = 19,
}


public enum LobbyPlatform
{
    None,
    Steam,
    PSN,
    Rail,
    XBone,
    Switch,
}