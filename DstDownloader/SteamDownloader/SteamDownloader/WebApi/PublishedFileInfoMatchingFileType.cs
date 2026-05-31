namespace SteamDownloader.WebApi;

public enum PublishedFileInfoMatchingFileType
{
    /// <summary>
    /// 物品
    /// </summary>
    Items = 0,
    /// <summary>
    /// 一套创意工坊物品
    /// </summary>
    Collections = 1,
    /// <summary>
    /// 艺术作品
    /// </summary>
    Art = 2,//。
    /// <summary>
    /// 视频
    /// </summary>
    Videos = 3,//。
    /// <summary>
    /// 屏幕截图
    /// </summary>
    Screenshots = 4,
    /// <summary>
    /// 可以被加入收藏的物品
    /// </summary>
    CollectionEligible = 5,
    /// <summary>
    /// 未使用
    /// </summary>
    Games = 6,//。
    /// <summary>
    /// 未使用
    /// </summary>
    Software = 7,
    /// <summary>
    /// 未使用
    /// </summary>
    Concepts = 8,
    /// <summary>
    /// 未使用
    /// </summary>
    GreenlightItems = 9,
    /// <summary>
    /// 指南
    /// </summary>
    AllGuides = 10,
    /// <summary>
    /// Steam 网页指南
    /// </summary>
    WebGuides = 11,//。
    /// <summary>
    /// 应用程序集成指南
    /// </summary>
    IntegratedGuides = 12,
    UsableInGame = 13,
    /// <summary>
    /// 需要投票决定是否出售的创意工坊商品
    /// </summary>
    Merch = 14,
    /// <summary>
    /// Steam 控制器绑定
    /// </summary>
    ControllerBindings = 15,
    /// <summary>
    /// 内部使用
    /// </summary>
    SteamworksAccessInvites = 16,
    /// <summary>
    /// 可以在游戏内出售的创意工坊物品
    /// </summary>
    Items_Mtx = 17,//。
    /// <summary>
    /// 可以立即被用户使用的创意工坊物品
    /// </summary>
    Items_ReadyToUse = 18,
    WorkshopShowcase = 19,
    /// <summary>
    /// 完全由游戏管理，不由用户管理，且不显示在网页上
    /// </summary>
    GameManagedItems = 20,
}