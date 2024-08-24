using DstServerQuery.Models;
using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers.Converters;

public class PlayersInfoWitTranslateConverter : JsonConverter<LobbyPlayerInfo[]>
{
    public static readonly FrozenDictionary<string, string> PrefabTranslations = new Dictionary<string, string>
    {
        {"waxwell", "麦斯威尔"},
        {"wendy", "温蒂"},
        {"wes", "韦斯"},
        {"willow", "薇洛"},
        {"wilson", "威尔逊"},
        {"winona", "薇诺娜"},
        {"wolfgang", "沃尔夫冈"},
        {"woodie", "伍迪"},
        {"wortox", "沃拓克斯" }, //恶魔
        {"wx-78", "WX-78"},
        {"wickerbottom", "薇克巴顿"},
        {"wathgrithr", "女武神"},
        {"wanda", "旺达"},
        {"wormwood", "沃姆伍德" }, //植物人
        {"walter", "沃尔特"},
        {"webber", "韦伯"},
        {"wurt", "沃特"}, //鱼人
        {"warly", "沃利" },

        { "yangjian", "杨戬" }, //神话
        { "monkey_king", "孙悟空" }, //神话
        { "myth_yutu", "玉兔" }, //神话
        { "white_bone", "白骨夫人" }, //神话

        { "xuaner", "璇儿" }, //璇儿
    }.ToFrozenDictionary();

    public override LobbyPlayerInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, LobbyPlayerInfo[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", item.Name);
            writer.WriteString("Color", item.Color);
            writer.WriteNumber("EventLevel", item.EventLevel);
            writer.WriteString("NetId", item.NetId);
            if (PrefabTranslations.TryGetValue(item.Prefab, out var translation))
            {
                writer.WriteString("Prefab", translation);
            }
            else
            {
                writer.WriteString("Prefab", item.Prefab);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}