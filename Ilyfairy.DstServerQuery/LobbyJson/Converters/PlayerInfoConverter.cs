﻿using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Utils;
using Neo.IronLua;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class PlayersInfoConverter : JsonConverter<List<LobbyPlayerInfo>>
{
    private static readonly Dictionary<string, string> PrefabName = new Dictionary<string, string>()
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
        {"WX-78", "WX-78"},
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
    };
    public override List<LobbyPlayerInfo>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) //不是字符串
        {
            _ = JsonNode.Parse(ref reader);
            return null;
        }
        if(reader.ValueSpan.Length > 100000) return new(); // 玩家列表过长, 不解析

        var playerLua = reader.GetString();
        if (playerLua == null) return null;

        if (playerLua is "return {  }") //玩家列表是空的
        {
            return new();
        }

        //将lua解析为LuauTable
        LuaTable? table = null;
        try
        {
            LuaResult r = LuaTempEnvironment.Instance.DoChunk(playerLua, "getplayers");
            table = r.Values.FirstOrDefault() as LuaTable;
            if (table == null) return new();
        }
        catch
        {
            return new();
        }
        
        List<LobbyPlayerInfo> list = new(8);
        foreach (var item in table.Select(v => v.Value as LuaTable))
        {
            if (item is null) continue;
            LobbyPlayerInfo info = new();
            info.Color = item.GetOptionalValue("colour", "#000000", true); //玩家文字颜色
            info.EventLevel = item.GetOptionalValue("eventlevel", -1, true);
            info.Name = item.GetOptionalValue("name", "未知", true); //玩家名

            string netidtemp = item.GetOptionalValue("netid", "-1", true); //玩家ID
            
            //分割ID只需要后半部分
            if (netidtemp.IndexOf(':') is int index && index != -1)
            {
                info.NetId = netidtemp[(index - 1)..];
            }
            else
            {
                info.NetId = netidtemp;
            }

            string prefab = item.GetOptionalValue("prefab", "未知", true); //玩家选择的角色, 如果没有选择角色则为空字符串
            info.Prefab = PrefabName.TryGetValue(prefab, out var p) ? p : prefab; //翻译角色名
            list.Add(info);
        }
        
        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<LobbyPlayerInfo> value, JsonSerializerOptions options)
    {
        //writer.WriteStringValue(value);
        throw new NotSupportedException();
    }
}
