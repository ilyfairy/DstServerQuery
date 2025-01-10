using DstServerQuery.Models;
using DstServerQuery.Services;
using MoonSharp.Interpreter;
using Neo.IronLua;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DstServerQuery.Helpers;

public static partial class DstConverterHelper
{
    private static ConcurrentDictionary<string, string> PlayerColorCache { get; } = new();
    private static ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> PlayerColorCacheAlternateLookup { get; }
    
    private static ConcurrentDictionary<string, string> PlayerPrefabCache { get; } = new();
    private static ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> PlayerPrefabCacheAlternateLookup { get; }

    public static GeoIPService? GeoIPService { get; set; }

    private static readonly IPAddressInfo _localhost = new()
    {
        CountryInfo = null,
        IPAddress = IPAddress.Loopback,
    };

    static DstConverterHelper()
    {
        PlayerColorCacheAlternateLookup = PlayerColorCache.GetAlternateLookup<ReadOnlySpan<char>>();
        PlayerPrefabCacheAlternateLookup = PlayerPrefabCache.GetAlternateLookup<ReadOnlySpan<char>>();
    }

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

    public static ReadOnlySpan<char> RemovePrefixColon(ReadOnlySpan<char> str)
    {
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

    public static LobbyDaysInfo? ParseDays(ReadOnlySpan<char> luaDayCode)
    {
        LobbyDaysInfo info = new();
        var dayOk = false;
        var daysElapsedInSeasonOk = false;
        var daysLeftInSeasonOk = false;
        foreach (var item in LuaKeyValuePairRegex().EnumerateMatches(luaDayCode))
        {
            const string Day = "day";
            const string DaysElapsedInSeason = "dayselapsedinseason";
            const string DaysLeftInSeason = "daysleftinseason";
            var span = luaDayCode.Slice(item.Index, item.Length);
            Span<Range> range2 = [default, default];
            var testSplitCount = span.Split(range2, '=');
            Debug.Assert(testSplitCount == 2);
            var number = int.Parse(span[range2[1]].Trim());
            if (span[range2[0]].Trim().SequenceEqual(Day))
            {
                dayOk = true;
                info.Day = number;
            }
            else if (span[range2[0]].Trim().SequenceEqual(DaysElapsedInSeason))
            {
                daysElapsedInSeasonOk = true;
                info.DaysElapsedInSeason = number;
            }
            else if (span[range2[0]].Trim().SequenceEqual(DaysLeftInSeason))
            {
                daysLeftInSeasonOk = true;
                info.DaysLeftInSeason = number;
            }
        }
        if (dayOk && daysElapsedInSeasonOk && daysLeftInSeasonOk)
        {
            return info;
        }
        return null;
    }

    public static LobbyDaysInfo? ParseDays(ReadOnlyMemory<char> luaDayCode)
    {
        // fallback
        LobbyDaysInfo info = new();
        Table? table = LuaTempEnvironment.Instance.DoChunk(luaDayCode).Table;
        if(table is null) return null;

        info.Day = (int)table.Get("day").Number;
        info.DaysElapsedInSeason = (int)table.Get("dayselapsedinseason").Number;
        info.DaysLeftInSeason = (int)table.Get("daysleftinseason").Number;
        return info;
    }

    public static LobbyPlayerInfo[]? ParsePlayers(ReadOnlySpan<char> playerLuaCode)
    {
        List<LobbyPlayerInfo>? regexPlayers = null;

        Span<Range> ranges = stackalloc Range[8]; // lines
        Span<Range> kvRange = [default, default];
        bool isRegexFail = false;
        foreach (var item in PlayerObjectRegex().EnumerateMatches(playerLuaCode))
        {
            var itemSpan = playerLuaCode.Slice(item.Index, item.Length);
            var lineCount = itemSpan.SplitAny(ranges, ['\r', '\n']);
            Debug.Assert(lineCount < 8);
            regexPlayers ??= new();
            LobbyPlayerInfo player = new();
            var colorOk = false;
            var eventLevelOk = false;
            var nameOk = false;
            var netIdOk = false;
            var prefabOk = false;
            foreach (var range in ranges[..lineCount])
            {
                var kvItem = itemSpan[range];
                if (kvItem.Contains('='))
                {
                    var testKvSplitLen = kvItem.Split(kvRange, '=');
                    Debug.Assert(testKvSplitLen == 2);
                    var k = kvItem[kvRange[0]].Trim();
                    var v = kvItem[kvRange[1]].Trim().TrimEnd(',');
                    if (k.SequenceEqual("colour"))
                    {
                        var colour = v.Trim('"');

                        if (PlayerColorCacheAlternateLookup.TryGetValue(colour, out var cachedColor))
                        {
                            player.Color = cachedColor;
                        }
                        else
                        {
                            cachedColor = colour.ToString();
                            player.Color = cachedColor;
                            PlayerColorCache.TryAdd(cachedColor, cachedColor);
                        }
                        colorOk = true;
                    }
                    else if (k.SequenceEqual("eventlevel"))
                    {
                        var eventlevel = int.Parse(v);
                        player.EventLevel = eventlevel;
                        eventLevelOk = true;
                    }
                    else if (k.SequenceEqual("name"))
                    {
                        var name = v.Trim('"').ToString();
                        player.Name = name;
                        nameOk = true;
                    }
                    else if (k.SequenceEqual("netid")) // 可能有冒号
                    {
                        var netid = DstConverterHelper.RemovePrefixColon(v.Trim('"')).ToString();
                        player.NetId = netid;
                        netIdOk = true;
                    }
                    else if (k.SequenceEqual("prefab"))
                    {
                        var prefab = v.Trim('"');
                        if (PlayerColorCacheAlternateLookup.TryGetValue(prefab, out var cachedPrefab))
                        {
                            player.Color = cachedPrefab;
                        }
                        else
                        {
                            cachedPrefab = prefab.ToString();
                            player.Prefab = cachedPrefab;
                            PlayerColorCache.TryAdd(cachedPrefab, cachedPrefab);
                        }
                        prefabOk = true;
                    }
                }
            }
            var isAllOk = colorOk && eventLevelOk && nameOk && netIdOk && prefabOk;
            Debug.Assert(isAllOk);
            if (isAllOk)
            {
                regexPlayers.Add(player);
            }
            else
            {
                isRegexFail = true;
                break;
            }
        }

        if (!isRegexFail && regexPlayers != null) // 没有匹配失败 && 有匹配项
        {
            return regexPlayers.ToArray();
        }

        return null;
    }

    public static LobbyPlayerInfo[]? ParsePlayers(ReadOnlyMemory<char> playerLuaCode)
    {
        // fallback
        if (playerLuaCode.Equals("return {  }")) //玩家列表是空的
        {
            return [];
        }

        //将lua解析为LuauTable
        Table? table = null;
        table = LuaTempEnvironment.Instance.DoChunk(playerLuaCode)?.Table; // may throw
        if (table == null)
            return null;

        var players = new LobbyPlayerInfo[table.Length];
        int index = 0;
        foreach (var item in table.Values.Select(v => v.Table))
        {
            LobbyPlayerInfo info = new();
            info.Color = item.Get("colour").String ?? "000000"; //玩家文字颜色
            info.EventLevel = (int)(item.Get("eventlevel")?.Number ?? -1);
            info.Name = item.Get("name").String ?? ""; //玩家名

            string? netidtemp = item.Get("netid").String; //玩家ID

            Debug.Assert(netidtemp != null, "玩家ID不明确");

            //分割ID只需要后半部分
            info.NetId = RemovePrefixColon(netidtemp);

            info.Prefab = item.Get("prefab").String ?? ""; //玩家选择的角色, 如果没有选择角色则为空字符串
            players[index++] = info;
        }

        return players;
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

    [GeneratedRegex(@"return\s*\{\s*day\s*=\s*(\d+)\s*,\s*dayselapsedinseason\s*=\s*(\d+)\s*,\s*daysleftinseason\s*=\s*(\d+)\s*\}")]
    private static partial Regex DayRegex();

    [GeneratedRegex(@"workshop\-(\d+)")]
    private static partial Regex WorkshopRegex();

    [GeneratedRegex(@"([a-z]+)\s*=\s*(\d+)")]
    private static partial Regex LuaKeyValuePairRegex();

    [GeneratedRegex("""{[\n\s]*colour="[a-zA-Z0-9]+",[\n\s]*eventlevel=\d+,[\n\s]*name=".+",[\n\s]*netid=".+",[\n\s]*prefab=".+"[\n\s]*}""")]
    private static partial Regex PlayerObjectRegex();
}
