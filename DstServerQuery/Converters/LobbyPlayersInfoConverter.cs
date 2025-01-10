using DstServerQuery.Helpers;
using DstServerQuery.Models;
using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DstServerQuery.Converters;

public class LobbyPlayersInfoConverter : JsonConverter<LobbyPlayerInfo[]>
{
    private static ReadOnlySpan<byte> _emptyUtf8 => "return {  }"u8;

    public override LobbyPlayerInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new JsonException("不是一个字符串");
        }

        if (reader.ValueSpan.SequenceEqual(_emptyUtf8))
        {
            return [];
        }

        if (reader.ValueSpan.Length <= 256)
        {
            Span<char> buffer = stackalloc char[reader.ValueSpan.Length];
            var charsLen = reader.CopyString(buffer);
            if (DstConverterHelper.ParsePlayers(buffer[..charsLen]) is { } players)
            {
                return players;
            }
            else
            {
                using TempUtf8JsonString str = TempUtf8JsonString.From(reader);
                return DstConverterHelper.ParsePlayers(str.String);
            }
        }
        else
        {
            using TempUtf8JsonString str = TempUtf8JsonString.From(reader);
            if (DstConverterHelper.ParsePlayers(str.String.Span) is { } players)
            {
                return players;
            }
            else
            {
                return DstConverterHelper.ParsePlayers(str.String);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, LobbyPlayerInfo[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
        //writer.WriteStartArray();
        //foreach (var item in value)
        //{
        //    writer.WriteStartObject();
        //    writer.WriteString("Name", item.Name);
        //    writer.WriteString("Color", item.Color);
        //    writer.WriteNumber("EventLevel", item.EventLevel);
        //    writer.WriteString("NetId", item.NetId);
        //    writer.WriteString("Prefab", item.Prefab);
        //    writer.WriteEndObject();
        //}
        //writer.WriteEndArray();
    }
}

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