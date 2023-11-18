using Ilyfairy.DstServerQuery.Models;
using Serilog;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;
public partial class LobbyModInfoConverter : JsonConverter<LobbyModInfo[]>
{
    public override LobbyModInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return null;
        }

        List<string> mods = new(16);
        try
        {
            foreach (var item in JsonNode.Parse(ref reader)?.AsArray() ?? throw new())
            {
                if (item is null) return null;
                mods.Add(item.ToString());
            }
        }
        catch (Exception)
        {
            Log.Warning("ModJson解析异常");
            return null;
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
                    Log.Warning("Mod解析失败: {Message}", e.Message);
                }
            }
        }
        else
        {
            //Log.Warning("ModItem不是5的倍数");
        }
        return infos.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, LobbyModInfo[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }

    [GeneratedRegex("workshop\\-(\\d+)")]
    private static partial Regex ModRegex();

    //[GeneratedRegex("workshop-(\\d+)")]
    //private static partial Regex WorkshopRegex();
}
