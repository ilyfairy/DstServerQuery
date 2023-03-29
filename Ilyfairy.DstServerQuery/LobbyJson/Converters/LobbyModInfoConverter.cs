using Ilyfairy.DstServerQuery.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;
public partial class LobbyModInfoConverter : JsonConverter<List<LobbyModInfo>>
{
    public override List<LobbyModInfo>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return new();
        }

        List<string> mods = new(16);
        try
        {
            foreach (var item in JsonNode.Parse(ref reader)?.AsArray())
            {
                if (item is null) return new();
                mods.Add(item.ToString());
            }
        }
        catch (Exception)
        {
            Console.WriteLine("ModJson解析异常");
            return new();
        }
        
        List<LobbyModInfo> infos = new(mods.Count / 5 + 1);
        if (mods.Count(v => v is "True" or "False" or "true" or "false") == mods.Count / 5)
        {
            for (int i = 0; i < mods.Count / 5; i++)
            {
                try
                {
                    var mod = new LobbyModInfo();
                    //mod.Id = base_mods_info[i * 5 + 0];
                    var work = mods[i * 5 + 0];
                    var matchId = ModRegex().Match(work);
                    if (!matchId.Success) continue;
                    var idParsed = long.TryParse(matchId.Groups[1].Value, out var id);
                    mod.Id = id;
                    mod.Name = mods[i * 5 + 1];
                    mod.NewVersion = mods[i * 5 + 2];
                    mod.CurrentVersion = mods[i * 5 + 3];
                    mod.IsClientDownload = bool.Parse(mods[i * 5 + 4]);
                    if (idParsed && mod.Name != null)
                    {
                        infos.Add(mod);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Mod解析失败: {e.Message}");
                }
            }
        }
        else
        {
            //Console.WriteLine("moditem不是5的倍数");
        }
        return infos;
    }

    public override void Write(Utf8JsonWriter writer, List<LobbyModInfo> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    [GeneratedRegex("workshop\\-(\\d+)")]
    private static partial Regex ModRegex();

    //[GeneratedRegex("workshop-(\\d+)")]
    //private static partial Regex WorkshopRegex();
}
