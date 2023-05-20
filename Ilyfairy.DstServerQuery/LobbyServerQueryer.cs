using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery;

public class LobbyServerQueryer
{
    public ICollection<LobbyDetailsData> LobbyDetailDatas { get; private set; }
    public List<LobbyDetailsData> Result { get; private set; }
    /// <summary>
    /// 查询的键值
    /// </summary>
    private readonly Dictionary<string, string> queryKeyValue = new(StringComparer.OrdinalIgnoreCase);

    public bool IsDetails { get; set; } = false;
    public int Page { get; set; } = 0;
    public int PageCount { get; set; } = 100;
    public int CurrentPageCount { get; set; }
    public int AllQueryCount { get; set; }
    public int MaxPage { get; set; }
    public bool IsRegex { get; set; } = false;
    public bool IsPlayerId { get; set; } = false;
    public bool IsPlayerQuery { get; set; }
    public bool IsSortDescending { get; set; }
    public DateTime LastUpdate { get; set; }

    public HashSet<string>? PropertiesRemove { get; set; }

    private string? json;
    public string? Json
    {
        get
        {
            if (json == null)
            {
                JsonObject jsonString;
                if (IsDetails)
                {
                    jsonString = CreateJson<LobbyDetailsData>();
                }
                else if (IsPlayerQuery)
                {
                    jsonString = CreateJson<LobbyBriefsDataPlayers>();
                }
                else
                {
                    jsonString = CreateJson<LobbyBriefsData>();
                }
                json = jsonString.ToJsonString(DstJsonConverter.Options);
            }
            return json;
        }
    }


    public LobbyServerQueryer(ICollection<LobbyDetailsData> lobbyDetailDatas, IEnumerable<KeyValuePair<string, string>> queryKey, DateTime lastUpdate)
    {
        LastUpdate = lastUpdate;
        LobbyDetailDatas = lobbyDetailDatas;
        Result = LobbyDetailDatas.ToList();
        foreach (var item in queryKey)
        {
            this.queryKeyValue[item.Key] = item.Value;
        }
    }


    public void Query()
    {
        if (json != null) return;

        RowIdProc();
        IsDetailsProc();

        IsRegexProc();
        ServerNameProc();

        IPProc();
        PortProc();
        ConnectProc();
        IsDedicatedProc();
        HostKleiIdProc();
        IsModsProc();
        PlatformProc();
        PvpProc();
        GameModeProc();

        PlayerIsByIdProc();
        PlayerNameProc();

        VersionProc();
        PasswordProc();
        OfficialProc();
        TagsProc();
        DescProc();
        PausedProc();
        AllowNewPlayersProc();
        DayProc();
        OwnerNetidProc();
        ModsNameProc();
        ModsIdProc();
        CountryProc();
        SeasonProc();

        IsSortDescendingProc();
        SortProc();

        PageCountProc();
        PageProc();

        PropertiesRemoveProc();

        AllQueryCount = Result.Count;
        Result = Result.Skip(Page * PageCount).Take(PageCount).ToList();
        CurrentPageCount = Result.Count;
    }

    private JsonObject CreateJson<T>() where T : LobbyBriefsData
    {
        List<T> list = Result.Select(v => (v as T)!).ToList();

        JsonObject json = new();
        json.Add("DateTime", DateTime.Now);
        json.Add("LastUpdate", LastUpdate);
        json.Add("Count", CurrentPageCount);
        json.Add("AllCount", AllQueryCount);
        json.Add("MaxPage", MaxPage);
        json.Add("Page", Page);

        if (PropertiesRemove == null || PropertiesRemove.Count == 0)
        {
            json.Add("List", JsonValue.Create(list));
        }
        else
        {
            JsonArray array = new();
            foreach (var item in list)
            {
                var obj = JsonSerializer.SerializeToNode(item, DstJsonConverter.Options)?.AsObject();
                if (obj is null) continue;
                foreach (var property in PropertiesRemove)
                {
                    obj.Remove(property);
                }
                array.Add(obj);
            }
            json.Add("List", array);
        }

        return json;
    }

    private string? GetQueryValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (queryKeyValue.TryGetValue(key, out var val))
            {
                return val;
            }
        }
        return null;
    }
    private int? GetQueryValueToInt(params string[] keys)
    {
        if (int.TryParse(GetQueryValue(keys), out var val))
        {
            return val;
        }
        else
        {
            return null;
        }
    }
    private long? GetValueToLong(params string[] keys)
    {
        if (long.TryParse(GetQueryValue(keys), out var val))
        {
            return val;
        }
        else
        {
            return null;
        }
    }
    private bool? GetQueryValueToBool(params string[] keys)
    {
        if (bool.TryParse(GetQueryValue(keys), out var val))
        {
            return val;
        }
        else
        {
            return null;
        }
    }

    private void ReAdd(IEnumerable<LobbyDetailsData> data)
    {
        var tmp = data.ToArray();
        Result.Clear();
        Result.AddRange(tmp);
    }
    /// <summary>
    /// 是否是通过RowId查询
    /// </summary>
    private void RowIdProc()
    {
        if (GetQueryValue("RowId") is string rowid)
        {
            var tmp = LobbyDetailDatas.Where(v => v.RowId == rowid).FirstOrDefault();
            Result.Clear();
            if (tmp is LobbyDetailsData data)
            {
                Result.Add(data);
            }
        }
    }
    /// <summary>
    /// 是否返回详细信息
    /// </summary>
    private void IsDetailsProc()
    {
        if (GetQueryValueToBool("Details", "IsDetails") is bool val)
        {
            IsDetails = val;
        }
    }
    /// <summary>
    /// 一页的个数
    /// </summary>
    private void PageCountProc()
    {
        if (GetQueryValueToInt("PageCount") is int count)
        {
            PageCount = count;
            if (PageCount <= 0) PageCount = 1;
            if (PageCount > 1000) PageCount = 1000;
        }
    }
    /// <summary>
    /// 第多少页
    /// </summary>
    private void PageProc()
    {
        MaxPage = Result.Count / PageCount;
        if (Result.Count / (double)PageCount % 1 > 0) MaxPage++;
        MaxPage--;
        if (GetQueryValueToInt("Page") is int page)
        {
            Page = page;
            if (Page < 0) Page = 0;
            if (Page > MaxPage) Page = MaxPage;
        }
    }
    /// <summary>
    /// 服务器名称
    /// </summary>
    private void ServerNameProc()
    {
        if (GetQueryValue("Name", "ServerName") is string serverName)
        {
            if (serverName.Length == 0) return;
            LobbyDetailsData[] tmp;
            if (IsRegex)
            {
                try
                {
                    tmp = Result.Where(v => Regex.IsMatch(v.Name, serverName)).ToArray();
                }
                catch (Exception)
                {
                    tmp = Array.Empty<LobbyDetailsData>();
                }
            }
            else
            {
                tmp = Result.Where(v => v.Name.Contains(serverName, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            }
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 搜索服务器名称是否使用正则
    /// </summary>
    private void IsRegexProc()
    {
        if (GetQueryValueToBool("isregex") is bool isRegex)
        {
            IsRegex = isRegex;
        }
    }
    /// <summary>
    /// 服务器IP
    /// </summary>
    private void IPProc()
    {
        if (GetQueryValue("ip") is string ip)
        {
            var match = Regex.Match(ip, @"^((?:\d{1,3})|[*])[.]((?:\d{1,3})|[*])[.]((?:\d{1,3})|[*])[.]((?:\d{1,3})|[*])$");
            if (match.Success)
            {
                var v1 = match.Groups[1].Value;
                var v2 = match.Groups[2].Value;
                var v3 = match.Groups[3].Value;
                var v4 = match.Groups[4].Value;
                var tmp = new List<LobbyDetailsData>();
                foreach (var item in Result.ToList())
                {
                    if (Regex.Match(item.Address.IP, $@"^{(v1 == "*" ? "\\d{1,3}" : v1)}[.]{(v2 == "*" ? "\\d{1,3}" : v2)}[.]{(v3 == "*" ? "\\d{1,3}" : v3)}[.]{(v4 == "*" ? "\\d{1,3}" : v4)}$").Success)
                    {
                        tmp.Add(item);
                    };
                }
                ReAdd(tmp);
            }
            else
            {
                Result.Clear();
            }
        }
    }
    /// <summary>
    /// 服务器端口
    /// </summary>
    private void PortProc()
    {
        if (GetQueryValue("port") is string port)
        {
            bool isNot = false;
            if (port.StartsWith('!'))
            {
                isNot = true;
                port = port[1..];
            }

            if (int.TryParse(port, out int val))
            {
                var tmp = new List<LobbyDetailsData>();
                foreach (var item in Result)
                {
                    if (item.Port == val ^ isNot) tmp.Add(item);
                }
                Result.AddRange(tmp);
            }
        }

    }
    /// <summary>
    /// 房间的玩家连接数
    /// </summary>
    private void ConnectProc()
    {
        if (GetQueryValue("Connect", "Connected", "Connected") is string connected)
        {
            // > < = xx%
            var oper = "";
            if (connected.StartsWith(">="))
            {
                oper = ">=";
            }
            else if (connected.StartsWith("<="))
            {
                oper = "<=";
            }
            else if (connected.StartsWith("=="))
            {
                oper = "==";
            }
            else if (connected.StartsWith("="))
            {
                oper = "=";
            }
            else if (connected.StartsWith(">"))
            {
                oper = ">";
            }
            else if (connected.StartsWith("<"))
            {
                oper = "<";
            }
            var num = connected[oper.Length..];
            bool isPer = false;
            if (num.EndsWith('%'))
            {
                isPer = true;
                num = num[0..^1];
            }
            if (oper is "" or "=")
            {
                oper = "==";
            }

            if (ushort.TryParse(num, out var n))
            {
                var tmp = new List<LobbyDetailsData>();
                foreach (var item in Result)
                {
                    void Match(bool isAdd)
                    {
                        if (isAdd)
                            tmp.Add(item);
                    }
                    switch (oper)
                    {
                        case "==":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections == (n / 100.0));
                            else
                                Match(item.Connected == n);
                            break;
                        case ">":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections > (n / 100.0));
                            else
                                Match(item.Connected > n);
                            break;
                        case "<":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections < (n / 100.0));
                            else
                                Match(item.Connected < n);
                            break;
                        case ">=":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections >= (n / 100.0));
                            else
                                Match(item.Connected >= n);
                            break;
                        case "<=":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections <= (n / 100.0));
                            else
                                Match(item.Connected <= n);
                            break;
                        default:
                            break;
                    }
                }
                ReAdd(tmp);
            }
        }

    }
    /// <summary>
    /// 是否是专用服务器
    /// </summary>
    private void IsDedicatedProc()
    {
        if (GetQueryValueToBool("Dedicated", "IsDedicated") is bool isDedicated)
        {
            var tmp = Result.Where(v => v.Dedicated == isDedicated);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 房主KleiID
    /// </summary>
    private void HostKleiIdProc()
    {
        if (GetQueryValue("Host", "HostId", "Host", "KleiId", "HostKleiId") is string hostKleiId)
        {
            var tmp = Result.Where(v => v.Host == hostKleiId);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 是否启用Mod
    /// </summary>
    private void IsModsProc()
    {
        if (GetQueryValueToBool("Mods", "Mod", "IsMod", "Mods") is bool isMods)
        {
            var tmp = new List<LobbyDetailsData>();
            foreach (var item in Result)
            {
                if (item.Mods == isMods)
                {
                    tmp.Add(item);
                }
            }
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 平台
    /// </summary>
    private void PlatformProc()
    {
        if (GetQueryValue("Platform") is string platform)
        {
            var tmp = new List<LobbyDetailsData>();
            foreach (var item in Result)
            {
                switch (platform.ToLower())
                {
                    case "0" or "none": //未知
                        if (item.Platform == Platform.None)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "1" or "steam": //Steam
                        if (item.Platform == Platform.Steam)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "2" or "wegame": //WeGame
                        if (item.Platform is Platform.WeGame or Platform.QQGame)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "3" or "playstation": //PlayStation
                        if (item.Platform == Platform.PlayStation)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "4" or "xbox": //Xbox
                        if (item.Platform == Platform.Xbox)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "5" or "switch": //Xbox
                        if (item.Platform == Platform.Switch)
                        {
                            tmp.Add(item);
                        }
                        break;
                }
            }
            Result.Clear();
            Result.AddRange(tmp);
        }
    }
    /// <summary>
    /// 是否是PVP
    /// </summary>
    private void PvpProc()
    {
        if (GetQueryValueToBool("pvp", "ispvp") is bool pvp)
        {
            var tmp = Result.Where(v => v.PVP == pvp);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 是否倒序排序
    /// </summary>
    private void IsSortDescendingProc()
    {
        if (GetQueryValueToBool("IsSortDescending", "DescendingSort") is bool isSortDescending)
        {
            IsSortDescending = isSortDescending;
        }
    }
    /// <summary>
    /// 排序
    /// </summary>
    private void SortProc()
    {
        if (GetQueryValueToInt("Sort", "SortType") is int sortType)
        {
            //排序
            switch (sortType)
            {
                case 1: //房间名排序
                    if (IsSortDescending) Result = Result.OrderByDescending(v => v.Name).ToList();
                    else Result = Result.OrderBy(v => v.Name).ToList();
                    break;
                case 2: //房间人数排序
                    if (IsSortDescending) Result = Result.OrderByDescending(v => v.Connected).ToList();
                    else Result = Result.OrderBy(v => v.Connected).ToList();
                    break;
                default: //默认
                    if (IsSortDescending) Result = Result.OrderByDescending(v => v.Name.GetHashCode()).ToList();
                    else Result = Result.OrderBy(v => v.Name.GetHashCode()).ToList();
                    break;
            }
        }
    }
    /// <summary>
    /// 是否通过玩家ID搜索玩家
    /// </summary>
    private void PlayerIsByIdProc()
    {
        if (GetQueryValueToBool("PlayerIsById", "IsPlayerId") is bool val)
        {
            IsPlayerId = val;
        }
    }
    /// <summary>
    /// 搜索玩家
    /// </summary>
    private void PlayerNameProc()
    {
        if (GetQueryValue("Player", "PlayerName") is string playerName)
        {
            IsPlayerQuery = true;
            List<LobbyDetailsData> tmp = new();
            var playerCount = LobbyDetailDatas.Sum(v => v.Players?.Count ?? 0);
            var resultPlayerCount = Result.Sum(v => v.Players?.Count ?? 0);

            foreach (var server in Result)
            {
                if (server.Players is null) continue;
                foreach (var player in server.Players)
                {
                    if (IsPlayerId)
                    {
                        if (player.NetId.ToString().Contains(playerName, StringComparison.OrdinalIgnoreCase))
                        {
                            tmp.Add(server);
                        }
                    }
                    else
                    {
                        if (player.Name.ToString().Contains(playerName, StringComparison.OrdinalIgnoreCase))
                        {
                            tmp.Add(server);
                        }
                    }
                }
            }
            ReAdd(tmp.ToHashSet());
        }
    }
    /// <summary>
    /// 版本过滤
    /// </summary>
    private void VersionProc()
    {
        if (GetQueryValue("Version", "V", "Ver") is string version)
        {
            long? Version = null;
            if (version.Contains("last") || version.Contains("new"))
            {
                Version = 0;
            }
            else if (long.TryParse(version, out var ver))
            {
                Version = ver;
            }
            if (version is not null)
            {
                var tmp = Result.Where(v => v.Version == Version);
                ReAdd(tmp);
            }
        }
    }
    /// <summary>
    /// 是否有密码
    /// </summary>
    private void PasswordProc()
    {
        if (GetQueryValueToBool("Password", "IsPassword") is bool isPassword)
        {
            var tmp = Result.Where(v => v.Password == isPassword);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 是否是Klei官方服务器
    /// </summary>
    private void OfficialProc()
    {
        if (GetQueryValueToBool("IsOfficial", "Official") is bool official)
        {
            var tmp = Result.Where(v => v.KleiOfficial == official);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// Tags筛选
    /// </summary>
    private void TagsProc()
    {
        if (GetQueryValue("Tags", "Tag") is string tag)
        {
            if (tag.Contains(','))
            {
                var tags = tag.Split(',');
                var tmp = Result.Where(v => v.Tags.Any(t => tags.Contains(t)));
                ReAdd(tmp);
            }
            else
            {
                var tmp = Result.Where(v => v.Tags.Contains(tag));
                ReAdd(tmp);
            }
        }
    }
    private void GameModeProc()
    {
        if (GetQueryValue("GameMode", "Mode") is string gamemode)
        {
            var tmp = new List<LobbyDetailsData>();
            foreach (var item in Result)
            {
                switch (gamemode.ToLower())
                {
                    case "0" or "none": //未知
                        if (item.Mode == GameMode.unknown)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "1" or "survival": //
                        if (item.Mode == GameMode.survival)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "2" or "wilderness": //
                        if (item.Mode == GameMode.wilderness)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "3" or "endless": //
                        if (item.Mode == GameMode.endless)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "4" or "lavaarena": //
                        if (item.Mode == GameMode.lavaarena)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "5" or "quagmire": //
                        if (item.Mode == GameMode.quagmire)
                        {
                            tmp.Add(item);
                        }
                        break;
                }
            }
            Result.Clear();
            Result.AddRange(tmp);
        }
    }
    /// <summary>
    /// 简介
    /// </summary>
    private void DescProc()
    {
        if (GetQueryValue("Desc", "Describe") is string desc)
        {
            var tmp = Result.Where(v => v.Desc.Contains(desc));
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 服务器是否暂停
    /// </summary>
    private void PausedProc()
    {
        if (GetQueryValueToBool("Paused", "IsPaused") is bool isPaused)
        {
            var tmp = Result.Where(v => v.ServerPaused == isPaused);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 是否允许新玩家加入
    /// </summary>
    private void AllowNewPlayersProc()
    {
        if (GetQueryValueToBool("AllowNewPlayer", "IsAllowNewPlayer", "IsAllowPlayer") is bool isAllowPlayer)
        {
            var tmp = Result.Where(v => v.ServerPaused == isAllowPlayer);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// 天数查询
    /// </summary>
    private void DayProc()
    {
        if (GetQueryValue("Day", "Days") is string day)
        {
            // > < = xx%
            var oper = "";
            if (day.StartsWith(">="))
            {
                oper = ">=";
            }
            else if (day.StartsWith("<="))
            {
                oper = "<=";
            }
            else if (day.StartsWith("=="))
            {
                oper = "==";
            }
            else if (day.StartsWith("="))
            {
                oper = "=";
            }
            else if (day.StartsWith(">"))
            {
                oper = ">";
            }
            else if (day.StartsWith("<"))
            {
                oper = "<";
            }
            var num = day[oper.Length..];

            if (oper is "" or "=")
            {
                oper = "==";
            }

            if (ushort.TryParse(num, out var n))
            {
                var tmp = new List<LobbyDetailsData>();
                foreach (var item in Result)
                {
                    if (item.DaysInfo is null) continue;
                    void Match(bool isAdd)
                    {
                        if (isAdd)
                            tmp.Add(item);
                    }
                    switch (oper)
                    {
                        case "==":
                            Match(item.DaysInfo.Day == n);
                            break;
                        case ">":
                            Match(item.DaysInfo.Day > n);
                            break;
                        case "<":
                            Match(item.DaysInfo.Day < n);
                            break;
                        case ">=":
                            Match(item.DaysInfo.Day >= n);
                            break;
                        case "<=":
                            Match(item.DaysInfo.Day <= n);
                            break;
                        default:
                            break;
                    }
                }
                ReAdd(tmp);
            }
        }
    }
    /// <summary>
    /// 房主的的Netid查询
    /// </summary>
    private void OwnerNetidProc()
    {
        if (GetQueryValue("Netid", "OwnerNetid") is string ownerNetid)
        {
            var tmp = Result.Where(v => v.Desc.Contains(ownerNetid));
            ReAdd(tmp);
        }

    }
    /// <summary>
    /// 查询指定Mod的房间
    /// </summary>
    private void ModsNameProc()
    {
        if (GetQueryValue("ModName", "ModName") is string modName)
        {
            if (modName.Contains('|'))
            {
                var names = modName.Split('|');
                var tmp = Result.Where(server => server.ModsInfo.Any(mod => names.Any(name => mod.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))));
                ReAdd(tmp);
            }
            else
            {
                var tmp = Result.Where(v => v.Tags.Contains(modName));
                ReAdd(tmp);
            }
        }
    }
    /// <summary>
    /// 通过ModID查询指定的房间
    /// </summary>
    private void ModsIdProc()
    {
        if (GetQueryValue("ModsId", "ModId") is string modId)
        {
            if (modId.Contains(','))
            {
                List<long> ids = new();
                foreach (var item in modId.Split(','))
                {
                    if (long.TryParse(item, out var id))
                    {
                        ids.Add(id);
                    }
                }
                var tmp = Result.Where(server => server.ModsInfo.Any(mod => ids.Any(id => id == mod.Id)));
                ReAdd(tmp);
            }
            else if (modId.Contains('|'))
            {
                List<long> ids = new();
                foreach (var item in modId.Split('|'))
                {
                    if (long.TryParse(item, out var id))
                    {
                        ids.Add(id);
                    }
                }
                var tmp = Result.Where(server => server.ModsInfo.Any(mod => ids.Any(id => id == mod.Id)));
                ReAdd(tmp);
            }
            else
            {
                if (long.TryParse(modId, out var id))
                {
                    var tmp = Result.Where(v => v.ModsInfo.Any(v => v.Id == id));
                    ReAdd(tmp);
                }
            }
        }
    }

    private readonly Season[] seasons = Enum.GetValues<Season>();
    /// <summary>
    /// 季节过滤
    /// </summary>
    private void SeasonProc()
    {
        if (GetQueryValue("season", "季节") is string seasonString)
        {
            Season? season = null;
            foreach (var item in seasons)
            {
                if (string.Equals(item.ToString(), seasonString, StringComparison.OrdinalIgnoreCase))
                {
                    season = item;
                    break;
                }
            }

            season ??= seasonString switch
                {
                    "春" or "春天" => Season.spring,
                    "夏" or "夏天" => Season.summer,
                    "秋" or "秋天" => Season.autumn,
                    "冬" or "冬天" => Season.winter,
                    _ => null
                };

            if (season == null) return;
            var tmp = Result.Where(v => v.Season == season.Value);
            ReAdd(tmp);
        }
    }

    private void CountryProc()
    {
        if (GetQueryValue("Country") is string country)
        {
            var tmp = Result.Where(v => string.Equals(v.Country, country, StringComparison.OrdinalIgnoreCase));
            ReAdd(tmp);
        }
    }

    private void PropertiesRemoveProc()
    {
        if (GetQueryValue("PropertiesRemove", "KeyRemove") is string properties)
        {
            var split = properties.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length <= 0) return;
            PropertiesRemove = new();
            foreach (var item in split)
            {
                PropertiesRemove.Add(item);
            }
        }
    }
}

