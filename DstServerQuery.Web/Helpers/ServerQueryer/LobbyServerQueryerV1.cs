using DstServerQuery.Models;
using DstServerQuery.Models.Lobby;
using DstServerQuery.Models.Lobby.Interfaces.V1;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DstServerQuery.Web.Helpers.ServerQueryer;

public class LobbyServerQueryerV1
{
    //private static JsonSerializerOptions()

    public ICollection<LobbyServerDetailed> LobbyDetailDatas { get; private set; }
    public List<LobbyServerDetailed> Result { get; private set; }
    /// <summary>
    /// 查询的键值
    /// </summary>
    private readonly Dictionary<string, string> queryKeyValue = new(StringComparer.OrdinalIgnoreCase);

    public bool IsDetails { get; set; } = false;
    public int PageIndex { get; set; } = 0;
    public int PageCount { get; set; } = 100;
    public int CurrentPageCount { get; set; }
    public int AllQueryCount { get; set; }
    public int MaxPageIndex { get; set; }
    public bool IsRegex { get; set; } = false;
    public bool IsPlayerId { get; set; } = false;
    public bool IsPlayerQuery { get; set; }
    public bool IsSortDescending { get; set; }
    public DateTimeOffset LastUpdate { get; set; }

    public HashSet<string>? PropertiesRemove { get; set; }

    public LobbyServerQueryerV1(ICollection<LobbyServerDetailed> lobbyDetailDatas, IEnumerable<KeyValuePair<string, string>> queryKey, DateTimeOffset lastUpdate)
    {
        LastUpdate = lastUpdate;
        LobbyDetailDatas = lobbyDetailDatas;
        Result = LobbyDetailDatas.ToList();
        foreach (var item in queryKey)
        {
            queryKeyValue[item.Key] = item.Value;
        }
    }

    public string ToJson(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        JsonObject jsonString;
        if (IsDetails || IsPlayerQuery)
        {
            jsonString = CreateJson<ILobbyServerDetailedV1>(jsonSerializerOptions);
        }
        else
        {
            jsonString = CreateJson<ILobbyServerV1>(jsonSerializerOptions);
        }

        return jsonString.ToJsonString(jsonSerializerOptions);
    }


    public void Query()
    {
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

        HandleIsSortDescending();
        HandleSort();

        PageCountProc();
        PageProc();

        PropertiesRemoveProc();

        AllQueryCount = Result.Count;
        Result = Result.Skip(PageIndex * PageCount).Take(PageCount).ToList();
        CurrentPageCount = Result.Count;
    }

    private JsonObject CreateJson<T>(JsonSerializerOptions? jsonSerializerOptions = null) where T : class, ILobbyServerV1
    {
        List<T> list = Result.Select(v => (v as T)!).ToList();

        JsonObject json = new();
        json.Add("DateTime", DateTimeOffset.Now);
        json.Add("LastUpdate", LastUpdate);
        json.Add("Count", CurrentPageCount);
        json.Add("AllCount", AllQueryCount);
        json.Add("MaxPage", MaxPageIndex);
        json.Add("Page", PageIndex);

        if (PropertiesRemove == null || PropertiesRemove.Count == 0)
        {
            json.Add("List", JsonValue.Create(list));
        }
        else
        {
            JsonArray array = new();
            foreach (var item in list)
            {
                var obj = JsonSerializer.SerializeToNode(item, new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter() }
                })?.AsObject();
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

    private void ReAdd(IEnumerable<LobbyServerDetailed> data)
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
            if (tmp is LobbyServerDetailed data)
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
        MaxPageIndex = Result.Count / PageCount;
        if (Result.Count / (double)PageCount % 1 > 0) MaxPageIndex++;
        MaxPageIndex--;
        if (GetQueryValueToInt("Page", "PageIndex") is int page)
        {
            PageIndex = page;
            if (PageIndex < 0) PageIndex = 0;
            if (PageIndex > MaxPageIndex) PageIndex = MaxPageIndex;
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
            LobbyServerDetailed[] tmp;
            if (IsRegex)
            {
                try
                {
                    tmp = Result.Where(v => Regex.IsMatch(v.Name, serverName)).ToArray();
                }
                catch (Exception)
                {
                    tmp = Array.Empty<LobbyServerDetailed>();
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
                var tmp = new List<LobbyServerDetailed>();
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
                var tmp = new List<LobbyServerDetailed>();
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
                var tmp = new List<LobbyServerDetailed>();
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
                                Match((double)item.Connected / item.MaxConnections == n / 100.0);
                            else
                                Match(item.Connected == n);
                            break;
                        case ">":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections > n / 100.0);
                            else
                                Match(item.Connected > n);
                            break;
                        case "<":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections < n / 100.0);
                            else
                                Match(item.Connected < n);
                            break;
                        case ">=":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections >= n / 100.0);
                            else
                                Match(item.Connected >= n);
                            break;
                        case "<=":
                            if (isPer)
                                Match((double)item.Connected / item.MaxConnections <= n / 100.0);
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
            var tmp = Result.Where(v => v.IsDedicated == isDedicated);
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
            var tmp = new List<LobbyServerDetailed>();
            foreach (var item in Result)
            {
                if (item.IsMods == isMods)
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
            var tmp = new List<LobbyServerDetailed>();
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
            var tmp = Result.Where(v => v.IsPvp == pvp);
            ReAdd(tmp);
        }
    }

    /// <summary>
    /// 是否倒序排序
    /// </summary>
    private void HandleIsSortDescending()
    {
        if (GetQueryValueToBool("IsSortDescending", "DescendingSort", "IsSortDesc") is bool isSortDescending)
        {
            IsSortDescending = isSortDescending;
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    private void HandleSort()
    {
        if (GetQueryValue("Sort", "SortType") is not string sort) return;

        if (string.Equals(sort, "Name") || string.Equals(sort, "ServerName"))
        {
            if (IsSortDescending) Result = Result.OrderByDescending(v => v.Name).ToList();
            else Result = Result.OrderBy(v => v.Name).ToList();
        }
        else if (string.Equals(sort, "Player") || string.Equals(sort, "PlayerCount") || string.Equals(sort, "ConnectionCount") || string.Equals(sort, "Connected"))
        {
            if (IsSortDescending) Result = Result.OrderByDescending(v => v.Connected).ToList();
            else Result = Result.OrderBy(v => v.Connected).ToList();
        }
        else
        {
            if (IsSortDescending) Result = Result.OrderByDescending(v => v.Name.GetHashCode()).ToList();
            else Result = Result.OrderBy(v => v.Name.GetHashCode()).ToList();
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
            List<LobbyServerDetailed> tmp = new();
            var playerCount = LobbyDetailDatas.Sum(v => v.Players?.Length ?? 0);
            var resultPlayerCount = Result.Sum(v => v.Players?.Length ?? 0);

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
            var tmp = Result.Where(v => v.IsPassword == isPassword);
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
            var tmp = Result.Where(v => v.IsKleiOfficial == official);
            ReAdd(tmp);
        }
    }
    /// <summary>
    /// Tags筛选
    /// </summary>
    private void TagsProc()
    {
        if (GetQueryValue("Tags", "Tag") is string queryTag)
        {
            if (queryTag.Contains(','))
            {
                var queryTags = queryTag.Split(',');
                var tmp = Result.Where(v => v.Tags?.Any(t =>
                {
                    foreach (var item in queryTags)
                    {
                        if (t.Equals(item)) return true;
                    }
                    return false;
                }) == true);
                ReAdd(tmp);
            }
            else
            {
                var tmp = Result.Where(v => v.Tags?.Any(t => t.SequenceEqual(queryTag)) == true);
                ReAdd(tmp);
            }
        }
    }
    private void GameModeProc()
    {
        if (GetQueryValue("GameMode", "Mode") is string gamemode)
        {
            var tmp = new List<LobbyServerDetailed>();
            foreach (var item in Result)
            {
                switch (gamemode.ToLower())
                {
                    //case "0" or "none": //未知
                    //    if (item.Mode == GameMode.unknown)
                    //    {
                    //        tmp.Add(item);
                    //    }
                    //    break;
                    case "1" or "survival": //
                        if (item.Mode == LobbyGameMode.Survival)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "2" or "wilderness": //
                        if (item.Mode == LobbyGameMode.Wilderness)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "3" or "endless": //
                        if (item.Mode == LobbyGameMode.Endless)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "4" or "lavaarena": //
                        if (item.Mode == LobbyGameMode.Lavaarena)
                        {
                            tmp.Add(item);
                        }
                        break;
                    case "5" or "quagmire": //
                        if (item.Mode == LobbyGameMode.Quagmire)
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
            var tmp = Result.Where(v => v.Description?.Contains(desc) == true);
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
            var tmp = Result.Where(v => v.IsServerPaused == isPaused);
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
            var tmp = Result.Where(v => v.IsServerPaused == isAllowPlayer);
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
                var tmp = new List<LobbyServerDetailed>();
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
            var tmp = Result.Where(v => v.Description?.Contains(ownerNetid) == true);
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
                var tmp = Result.Where(server => server.ModsInfo?.Any(mod => names.Any(name => mod.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))) == true);
                ReAdd(tmp);
            }
            else
            {
                var tmp = Result.Where(v => v.ModsInfo?.Any(v => v.Name.Contains(modName)) == true);
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
                var tmp = Result.Where(server => server.ModsInfo?.Any(mod => ids.Any(id => id == mod.Id)) == true);
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

    // DOTO: 季节过滤
    //private readonly Season[] seasons = Enum.GetValues<Season>();
    /// <summary>
    /// 季节过滤
    /// </summary>
    private void SeasonProc()
    {
        if (GetQueryValue("season", "季节") is string seasonString)
        {
            LobbySeason? season = null;
            //foreach (var item in seasons)
            //{
            //    if (string.Equals(item.ToString(), seasonString, StringComparison.OrdinalIgnoreCase))
            //    {
            //        season = item;
            //        break;
            //    }
            //}

            season ??= seasonString switch
            {
                "春" or "春天" => LobbySeason.Spring,
                "夏" or "夏天" => LobbySeason.Summer,
                "秋" or "秋天" => LobbySeason.Autumn,
                "冬" or "冬天" => LobbySeason.Winter,
                _ => null
            };

            if (season == null) return;
            var tmp = Result.Where(v => v.Season == season);
            ReAdd(tmp);
        }
    }

    private void CountryProc()
    {
        if (GetQueryValue("Country") is string country)
        {
            var tmp = Result.Where(v => string.Equals(v.Address.IsoCode, country, StringComparison.OrdinalIgnoreCase));
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

