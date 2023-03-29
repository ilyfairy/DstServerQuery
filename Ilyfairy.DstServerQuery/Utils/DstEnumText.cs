using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Ilyfairy.DstServerQuery.Models;

namespace Ilyfairy.DstServerQuery.Utils;

/// <summary>
/// 定义了枚举对应的文本
/// </summary>
public class DstEnumText : Dictionary<Enum, string> 
{
    public static DstEnumText Instance => new Lazy<DstEnumText>(() => new DstEnumText()
    {
        { Day.@default, "默认" },
        { Day.longday, "长白天" },
        { Day.longdusk, "长黄昏" },
        { Day.longnight, "长夜晚" },
        { Day.noday, "无白天" },
        { Day.nodusk, "无黄昏" },
        { Day.nonight, "无夜晚" },
        { Day.onlyday, "仅白天" },
        { Day.onlydusk, "仅黄昏" },
        { Day.onlynight, "仅夜晚" },

        { Frequency.never, "无" },
        { Frequency.rare, "很少" },
        { Frequency.@default, "默认" },
        { Frequency.often, "较多" },
        { Frequency.always, "大量" },

        { ExtraStartingItems._0, "总是" },
        { ExtraStartingItems._5, "第5天后" },
        { ExtraStartingItems._10, "第10天后" },
        { ExtraStartingItems._15, "第15天后" },
        { ExtraStartingItems._20, "第20天后" },
        { ExtraStartingItems.none, "从不" },

        { DropEverythingOnDespawn.@default, "总是" },
        { DropEverythingOnDespawn.always, "所有" },

        { IntentionType.unknown, "未知" },
        { IntentionType.relaxed, "轻松" },
        { IntentionType.endless, "无尽" },
        { IntentionType.survival, "生存" },
        { IntentionType.wilderness, "荒野" },
        { IntentionType.lightsout, "暗无天日" },
        { IntentionType.cooperative, "合作" },
        { IntentionType.ooperative, "合作" },
        { IntentionType.social, "社交" },
        { IntentionType.madness, "疯狂" },
        { IntentionType.competitive, "竞争" },
        { IntentionType.oceanfishing, "海钓" },

        { Behaviour.never, "无" },
        { Behaviour.@default, "默认" },
        { Behaviour.always, "总是" },

        { Speed.none, "无" },
        { Speed.few, "慢" },
        { Speed.@default, "默认" },
        { Speed.many, "快" },
        { Speed.max, "极快" },

        { GrowthSpeed.never, "无" },
        { GrowthSpeed.veryslow, "极慢" },
        { GrowthSpeed.slow, "慢" },
        { GrowthSpeed.@default, "默认" },
        { GrowthSpeed.fast, "快" },
        { GrowthSpeed.veryfast, "极快" },

        { SpecialEvent.none, "无" },
        { SpecialEvent.auto, "自动" },
        { SpecialEvent.midsummer_cawnival, "盛夏鸦年华" },
        { SpecialEvent.hallowed_nights, "万圣节" },
        { SpecialEvent.winters_feast, "冬季盛宴" },
        { SpecialEvent.year_of_the_gobbler, "火鸡之年" },
        { SpecialEvent.year_of_the_varg, "座狼之年" },
        { SpecialEvent.year_of_the_pig_king, "猪王之年" },
        { SpecialEvent.year_of_the_carrat, "胡萝卜之年" },
        { SpecialEvent.year_of_the_beefalo, "皮弗娄牛之年" },
        { SpecialEvent.year_of_the_catcoon, "浣猫之年" },

        { IsExist.never, "无" },
        { IsExist.@default, "默认" },

        { SeasonalDuration.noseason, "无" },
        { SeasonalDuration.veryshortseason, "极短" },
        { SeasonalDuration.shortseason, "短" },
        { SeasonalDuration.@default, "默认" },
        { SeasonalDuration.longseason, "长" },
        { SeasonalDuration.verylongseason, "极长" },
        { SeasonalDuration.random, "随机" },

        { Season.autumn, "秋" },
        { Season.winter, "冬" },
        { Season.spring, "春" },
        { Season.summer, "夏" },
        { Season.autumnOrspring, "春活秋" },
        { Season.winterOrsummer, "冬季或夏季" },
        { Season.autumnOrwinterOrspringOrsummer, "随机" },

        { TaskSet.@default, "联机版" },
        { TaskSet.classic, "经典" },
        { TaskSet.cave_default, "洞穴" },

        { WorldSize.small, "小" },
        { WorldSize.medium, "中" },
        { WorldSize.@default, "大" },
        { WorldSize.huge, "巨大" },

        { Branching.never, "从不" },
        { Branching.least, "最少" },
        { Branching.@default, "默认" },
        { Branching.most, "最多" },
        { Branching.random, "随机" },

        { Quantity.never, "无" },
        { Quantity.rare, "很少" },
        { Quantity.uncommon, "较少" },
        { Quantity.@default, "默认" },
        { Quantity.often, "较多" },
        { Quantity.mostly, "很多" },
        { Quantity.always, "大量" },
        { Quantity.insane, "疯狂" },

        { OceanQuantity.ocean_never, "无" },
        { OceanQuantity.ocean_rare, "很少" },
        { OceanQuantity.ocean_uncommon, "较少" },
        { OceanQuantity.ocean_default, "默认" },
        { OceanQuantity.ocean_often, "较多" },
        { OceanQuantity.ocean_mostly, "很多" },
        { OceanQuantity.ocean_always, "大量" },
        { OceanQuantity.ocean_insane, "疯狂" },

        { PrefabswapsStart.classic, "经典" },
        { PrefabswapsStart.@default, "默认" },
        { PrefabswapsStart.highly_random, "非常随机" },

        { GameMode.unknown, "未知" },
        { GameMode.survival, "生存" },
        { GameMode.wilderness, "荒野" },
        { GameMode.endless, "无尽" },
        { GameMode.lavaarena, "熔炉" },
        { GameMode.quagmire, "暴食" },
        { GameMode.starving_floor, "StarvingFloor" },
        { GameMode.smashup, "Smashup" }
    }).Value;   

    private DstEnumText()
    {

    }

    public new string this[Enum e]
    {
        get
        {
            if (TryGetValue(e, out var value))
                return value;
            return string.Empty;
        }
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public string? TryGetValueOrDefault(Enum e, string? defaultValue = null)
    {
        if (TryGetValue(e, out var value))
            return value;
        return defaultValue;
    }
}
