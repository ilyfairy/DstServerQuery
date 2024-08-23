using System;
using System.Collections.Generic;
using System.Text;

namespace Ilyfairy.DstServerQuery.Models;

/// <summary>
/// 昼夜选项
/// </summary>
public enum Day
{
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 长白天
    /// </summary>
    longday,
    /// <summary>
    /// 长黄昏
    /// </summary>
    longdusk,
    /// <summary>
    /// 长夜晚
    /// </summary>
    longnight,
    /// <summary>
    /// 无白天
    /// </summary>
    noday,
    /// <summary>
    /// 无黄昏
    /// </summary>
    nodusk,
    /// <summary>
    /// 无夜晚
    /// </summary>
    nonight,
    /// <summary>
    /// 仅白天
    /// </summary>
    onlyday,
    /// <summary>
    /// 仅黄昏
    /// </summary>
    onlydusk,
    /// <summary>
    /// 仅夜晚
    /// </summary>
    onlynight,
}
/// <summary>
/// 频率
/// </summary>
public enum Frequency
{
    /// <summary>
    /// 无
    /// </summary>
    never,
    /// <summary>
    /// 很少
    /// </summary>
    rare,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 较多
    /// </summary>
    often,
    /// <summary>
    /// 大量
    /// </summary>
    always,
}
/// <summary>
/// 额外起始资源
/// </summary>
public enum ExtraStartingItems
{
    /// <summary>
    /// 总是
    /// </summary>
    _0,
    /// <summary>
    /// 第五天后
    /// </summary>
    _5,
    /// <summary>
    /// 第10天后
    /// </summary>
    _10,
    /// <summary>
    /// 第15天后
    /// </summary>
    _15,
    /// <summary>
    /// 第20天后
    /// </summary>
    _20,
    /// <summary>
    /// 从不
    /// </summary>
    none,
}
/// <summary>
/// 离开游戏后物品掉落
/// </summary>
public enum DropEverythingOnDespawn
{
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 所有
    /// </summary>
    always,
}

/// <summary>
/// 行为
/// </summary>
public enum Behaviour
{
    /// <summary>
    /// 无
    /// </summary>
    never,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 总是
    /// </summary>
    always,
}
/// <summary>
/// 速度
/// </summary>
public enum Speed
{
    /// <summary>
    /// 无
    /// </summary>
    none,
    /// <summary>
    /// 慢
    /// </summary>
    few,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 快
    /// </summary>
    many,
    /// <summary>
    /// 极快
    /// </summary>
    max,
}
/// <summary>
/// 生长速度
/// </summary>
public enum GrowthSpeed
{
    /// <summary>
    /// 无
    /// </summary>
    never,
    /// <summary>
    /// 极慢
    /// </summary>
    veryslow,
    /// <summary>
    /// 慢
    /// </summary>
    slow,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 快
    /// </summary>
    fast,
    /// <summary>
    /// 极快
    /// </summary>
    veryfast, 
}
/// <summary>
/// 特殊活动
/// </summary>
public enum SpecialEvent
{
    /// <summary>
    /// 无
    /// </summary>
    none,
    /// <summary>
    /// 自动
    /// </summary>
    auto,
    /// <summary>
    /// 盛夏鸦年华
    /// </summary>
    midsummer_cawnival,
    /// <summary>
    /// 万圣夜
    /// </summary>
    hallowed_nights,
    /// <summary>
    /// 冬季盛宴
    /// </summary>
    winters_feast,
    /// <summary>
    /// 火鸡之年
    /// </summary>
    year_of_the_gobbler,
    /// <summary>
    /// 座狼之年
    /// </summary>
    year_of_the_varg,
    /// <summary>
    /// 猪王之年
    /// </summary>
    year_of_the_pig_king,
    /// <summary>
    /// 胡萝卜之年
    /// </summary>
    year_of_the_carrat,
    /// <summary>
    /// 皮弗娄牛之年
    /// </summary>
    year_of_the_beefalo,
    /// <summary>
    /// 浣猫之年
    /// </summary>
    year_of_the_catcoon
}
/// <summary>
/// 是否存在
/// </summary>
public enum IsExist
{
    /// <summary>
    /// 无
    /// </summary>
    never,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
}
/// <summary>
/// 季节时长
/// </summary>
public enum SeasonalDuration
{
    /// <summary>
    /// 无
    /// </summary>
    noseason,
    /// <summary>
    /// 极短
    /// </summary>
    veryshortseason,
    /// <summary>
    /// 短
    /// </summary>
    shortseason,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 长
    /// </summary>
    longseason,
    /// <summary>
    /// 极长
    /// </summary>
    verylongseason,
    /// <summary>
    /// 随机
    /// </summary>
    random,
}
/// <summary>
/// 生物群落
/// </summary>
public enum TaskSet
{
    /// <summary>
    /// 联机版
    /// </summary>
    @default,
    /// <summary>
    /// 经典
    /// </summary>
    classic,
    /// <summary>
    /// 洞穴
    /// </summary>
    cave_default,
}
/// <summary>
/// 出生点
/// </summary>
public enum StartLocation
{
    /// <summary>
    /// 额外资源
    /// </summary>
    plus,
    /// <summary>
    /// 黑暗
    /// </summary>
    darkness,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
}
/// <summary>
/// 世界大小
/// </summary>
public enum WorldSize
{
    /// <summary>
    /// 小
    /// </summary>
    small,
    /// <summary>
    /// 中
    /// </summary>
    medium,
    /// <summary>
    /// 大
    /// </summary>
    @default,
    /// <summary>
    /// 巨大
    /// </summary>
    huge,
}
/// <summary>
/// 分支
/// </summary>
public enum Branching
{
    /// <summary>
    /// 从不
    /// </summary>
    never,
    /// <summary>
    /// 最少
    /// </summary>
    least,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 最多
    /// </summary>
    most,
    /// <summary>
    /// 随机
    /// </summary>
    random,
}
/// <summary>
/// 数量
/// </summary>
public enum Quantity
{
    /// <summary>
    /// 无
    /// </summary>
    never,
    /// <summary>
    /// 很少
    /// </summary>
    rare,
    /// <summary>
    /// 较少
    /// </summary>
    uncommon,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 较多
    /// </summary>
    often,
    /// <summary>
    /// 很多
    /// </summary>
    mostly,
    /// <summary>
    /// 大量
    /// </summary>
    always,
    /// <summary>
    /// 疯狂
    /// </summary>
    insane,
}
/// <summary>
/// 海洋数量
/// </summary>
public enum OceanQuantity
{
    /// <summary>
    /// 无
    /// </summary>
    ocean_never,
    /// <summary>
    /// 很少
    /// </summary>
    ocean_rare,
    /// <summary>
    /// 较少
    /// </summary>
    ocean_uncommon,
    /// <summary>
    /// 默认
    /// </summary>
    ocean_default,
    /// <summary>
    /// 较多
    /// </summary>
    ocean_often,
    /// <summary>
    /// 很多
    /// </summary>
    ocean_mostly,
    /// <summary>
    /// 大量
    /// </summary>
    ocean_always,
    /// <summary>
    /// 疯狂
    /// </summary>
    ocean_insane,
}
/// <summary>
/// 开始资源多样化
/// </summary>
public enum PrefabswapsStart
{
    /// <summary>
    /// 经典
    /// </summary>
    classic,
    /// <summary>
    /// 默认
    /// </summary>
    @default,
    /// <summary>
    /// 非常随机
    /// </summary>
    highly_random,
}
