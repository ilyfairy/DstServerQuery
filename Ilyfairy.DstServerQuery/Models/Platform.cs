namespace Ilyfairy.DstServerQuery.Models;

/// <summary>
/// 平台
/// </summary>
public enum Platform
{
    None = 0x00,
    Steam = 0x01,
    PlayStation = 0x02,
    WeGame = 0x04,
    QQGame = 0x08,
    Xbox = 0x10,
    Switch = 0x20,
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