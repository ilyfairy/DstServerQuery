using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Helpers;
using Neo.IronLua;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters;

public class PlayersInfoConverter : JsonConverter<LobbyPlayerInfo[]>
{
    public override LobbyPlayerInfo[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) //不是字符串
        {
            _ = JsonNode.Parse(ref reader);
            return [];
        }
        if (reader.ValueSpan.Length > 200000) return []; // 玩家列表过长, 不解析

        var playerLua = reader.GetString();
        if (playerLua == null) return [];

        if (playerLua is "return {  }") //玩家列表是空的
        {
            return [];
        }

        //将lua解析为LuauTable
        LuaTable? table = null;
        try
        {
            LuaResult r = LuaTempEnvironment.Instance.DoChunk(playerLua, "getplayers");
            table = r.Values.FirstOrDefault() as LuaTable;
            if (table == null) return [];
        }
        catch
        {
            return [];
        }

        List<LobbyPlayerInfo> list = new(table.Length);
        foreach (var item in table.Select(v => v.Value as LuaTable))
        {
            if (item is null) continue;
            LobbyPlayerInfo info = new();
            info.Color = item.GetOptionalValue("colour", "#000000", true); //玩家文字颜色
            info.EventLevel = item.GetOptionalValue("eventlevel", -1, true);
            info.Name = item.GetOptionalValue("name", "", true); //玩家名

            string? netidtemp = item.GetOptionalValue("netid", default(string), true); //玩家ID

            Debug.Assert(netidtemp != null, "玩家ID不明确");

            //分割ID只需要后半部分
            if (netidtemp.IndexOf(':') is int index && index != -1)
            {
                info.NetId = netidtemp[(index + 1)..];
            }
            else
            {
                info.NetId = netidtemp;
            }

            info.Prefab = item.GetOptionalValue("prefab", "", true); //玩家选择的角色, 如果没有选择角色则为空字符串
            list.Add(info);
        }

        return list.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, LobbyPlayerInfo[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}


public class PlayersInfoWitTranslateConverter : PlayersInfoConverter
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