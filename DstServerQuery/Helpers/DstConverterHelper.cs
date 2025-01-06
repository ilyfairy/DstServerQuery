using DstServerQuery.Models;
using DstServerQuery.Services;
using Neo.IronLua;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DstServerQuery.Helpers;

public static partial class DstConverterHelper
{
    public static Dictionary<ReadOnlyMemory<char>, string> TagsCache { get; } = new(new MemoryCharEqualityComparer());

    public static GeoIPService? GeoIPService { get; set; }

    private static readonly IPAddressInfo _localhost = new()
    {
        CountryInfo = null,
        IPAddress = IPAddress.Loopback,
    };


    [return: NotNullIfNotNull(nameof(str))]
    public static string? RemovePrefixColon(string? str)
    {
        if (str is null) return null;
        var perfixIndex = str.IndexOf(':');
        if (perfixIndex != -1)
        {
            return str[(perfixIndex + 1)..];
        }
        return str;
    }

    [return: NotNullIfNotNull(nameof(worldLevelRawItem))]
    public static WorldLevelItem[]? WorldLevelRawToArray(Dictionary<string, WorldLevelRawItem>? worldLevelRawItem)
    {
        return worldLevelRawItem?.Select(v => WorldLevelItem.FromRaw(v.Value)).ToArray();
    }

    public static IPAddressInfo ParseAddress(string ip)
    {
        if (ip == "127.0.0.1") return _localhost;

        IPAddressInfo info;

        if (IPAddress.TryParse(ip, out IPAddress? ipAddress) is false)
        {
            return _localhost;
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_toString")] // NOTE: unsafe
        static extern ref string IPAddress_toString(IPAddress ipAddress);

        IPAddress_toString(ipAddress) = ip;

        try
        {
            if (GeoIPService?.TryCity(ipAddress, out var city) == true)
            {
                info = new IPAddressInfo()
                {
                    IPAddress = ipAddress,
                    CountryInfo = city?.Country,
                    CityInfo = city?.City,
                };
            }
            else
            {
                info = new IPAddressInfo()
                {
                    IPAddress = ipAddress,
                };
            }
        }
        catch
        {
            info = new IPAddressInfo()
            {
                CountryInfo = null,
                IPAddress = ipAddress,
            };
        }

        return info;
    }

    public static LobbyDaysInfo? ParseDays(string? luaDayCode)
    {
        if (luaDayCode is null) return null;

        //var match = DayRegex().Match(luaDayCode);
        LuaTable? table = LuaTempEnvironment.Instance.DoChunk(luaDayCode, "day").Values.FirstOrDefault() as LuaTable;
        if(table is null) return null;

        LobbyDaysInfo info = new();
        info.Day = (int)table.GetValue("day");
        info.DaysElapsedInSeason = (int)table.GetValue("dayselapsedinseason");
        info.DaysLeftInSeason = (int)table.GetValue("daysleftinseason");
        return info;
    }

    public static LobbyModInfo[]? ParseMods(object[]? mods)
    {
        if (mods is null) return null;
        List<LobbyModInfo> infos = new(mods.Length / 5 + 1);
        if (mods.Count(v => v is "True" or "False" or "true" or "false" or true or false) == mods.Length / 5)
        {
            for (int i = 0; i < mods.Length / 5; i++)
            {
                var mod = new LobbyModInfo();
                //mod.Id = base_mods_info[i * 5 + 0];
                var work = mods[i * 5 + 0].ToString()!;
                var matchId = WorkshopRegex().Match(work);
                if (!matchId.Success)
                    continue;
                var idParsed = long.TryParse(matchId.Groups[1].Value, out var id);

                mod.Id = id;
                mod.Name = mods[i * 5 + 1]?.ToString()!;
                mod.NewVersion = mods[i * 5 + 2]?.ToString();
                mod.CurrentVersion = mods[i * 5 + 3]?.ToString();
                if (mods[i * 5 + 4] is bool isClientDownload)
                {
                    mod.IsClientDownload = isClientDownload;
                }
                else
                {
                    mod.IsClientDownload = bool.Parse(mods[i * 5 + 4].ToString()!);
                }
                if (idParsed && mod.Name != null)
                {
                    infos.Add(mod);
                }
            }
        }
        else
        {
            //Log.Warning("ModItem不是5的倍数");
        }
        return infos.ToArray();
    }

    public static LobbyPlayerInfo[]? ParsePlayers(string? playerLuaCode)
    {
        if (playerLuaCode == null) return null;

        if (playerLuaCode is "return {  }") //玩家列表是空的
        {
            return [];
        }

        //将lua解析为LuauTable
        LuaTable? table = null;
        try
        {
            LuaResult r = LuaTempEnvironment.Instance.DoChunk(playerLuaCode, "getplayers");
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
            info.NetId = RemovePrefixColon(netidtemp);

            info.Prefab = item.GetOptionalValue("prefab", "", true); //玩家选择的角色, 如果没有选择角色则为空字符串
            list.Add(info);
        }

        return list.ToArray();
    }

    [return: NotNullIfNotNull(nameof(tagsString))]
    public static string[]? ParseTags(string? tagsString)
    {
        if (tagsString is null) return null;
        if (tagsString.Length == 0) return Array.Empty<string>();

        var span = tagsString.AsSpan();
        var tagsMaxCount = Utils.GetCharCount(tagsString, ',') + 1;
        Span<Range> ranges = tagsMaxCount < 512 ? stackalloc Range[tagsMaxCount] : new Range[tagsMaxCount];
        var tagsCount = span.Split(ranges, ',', StringSplitOptions.RemoveEmptyEntries);
        string[] tags = new string[tagsCount];

        try
        {
            for (int i = 0; i < tagsCount; i++)
            {
                var range = ranges[i];
                var tagMemory = tagsString.AsMemory(range.Start.Value, range.End.Value - range.Start.Value).Trim();

                if (TagsCache.TryGetValue(tagMemory, out var tag))
                {
                    tags[i] = tag;
                }
                else
                {
                    var str = tagMemory.ToString();
                    TagsCache[str.AsMemory()] = str;
                    tags[i] = str;
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
        return tags;
    }

    [return: NotNullIfNotNull(nameof(tagsString))]
    public static ReadOnlyMemory<char>[]? ParseTagsAsMemory(string? tagsString)
    {
        if (tagsString is null) return null;
        if (tagsString.Length == 0) return [];

        var span = tagsString.AsSpan();
        var tagsMaxCount = Utils.GetCharCount(tagsString, ',') + 1;
        Span<Range> ranges = tagsMaxCount < 512 ? stackalloc Range[tagsMaxCount] : new Range[tagsMaxCount];
        var tagsCount = span.Split(ranges, ',', StringSplitOptions.RemoveEmptyEntries);
        var tags = new ReadOnlyMemory<char>[tagsCount];

        for (int i = 0; i < tagsCount; i++)
        {
            var range = ranges[i];
            var tagMemory = tagsString.AsMemory(range.Start.Value, range.End.Value - range.Start.Value).Trim();
            tags[i] = tagMemory;
        }
        return tags;
    }







    [GeneratedRegex(@"return\s*\{\s*day=(\d+),\s*dayselapsedinseason=(\d+),\s*daysleftinseason=(\d+)\s*\}")]
    private static partial Regex DayRegex();

    [GeneratedRegex(@"workshop\-(\d+)")]
    private static partial Regex WorkshopRegex();
}
