using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer.JsonConverters;
using Medallion.Collections;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery.Web.Helpers.ServerQueryer;

public class LobbyServerQueryerV2
{
    private readonly QueryParams queryParams;
    private readonly ICollection<LobbyServerDetailed> servers;

    private IEnumerable<LobbyServerDetailed> current;

    private readonly long? latest;

    public RegexOptions RegexOptions => queryParams.IgnoreCase is null or true ? RegexOptions.IgnoreCase : RegexOptions.None;
    public StringComparison StringComparison => queryParams.IgnoreCase is null or true ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public LobbyServerQueryerV2(QueryParams queryParams, ICollection<LobbyServerDetailed> servers, long? latest = null)
    {
        this.queryParams = queryParams;
        this.servers = servers;
        current = servers;
        this.latest = latest;
    }

    public ICollection<LobbyServerDetailed> Query()
    {
        HandleServerName();
        HandlePlayerName();
        HandleSeason();
        HandleVersion();
        HandleGameMode();
        HandleIntent();
        HandleIP();
        HandleHost();
        HandlePlatform();
        HandleCountry();
        HandleModsName();
        HandleModsId();
        HandleIsOfficial();
        HandleIsPassword();
        HandleIsPvp();
        HandleDays();
        HandleDaysInSeason();
        HandleDescription();
        HandleTags();
        HandleMaxConnection();
        HandleConnected();
        HandlePlayerPrefab();

        HandleSort();

        var result = current.ToArray();

        return result;
    }


    private void HandleSort()
    {
        current = current.OrderByDescending(v => v.Name.GetHashCode());
        if (queryParams.Sort is null || queryParams.Sort.Value.Value is null)
            return;

        var sort = queryParams.Sort.Value;

        var sorts = sort.Value.Where(v => v is not null).Take(10).ToArray();
    
        foreach (var item in sorts)
        {
            string? sortName = item;
            bool isDesc = false;

            if (item is ['+', .. var val])
            {
                sortName = val;
                isDesc = false;
            }
            else if (item is ['-', .. var val2])
            {
                sortName = val2;
                isDesc = true;
            }

            void HandleOrder<T>(Func<LobbyServerDetailed, T> func)
            {
                if (isDesc)
                {
                    current = current.OrderByDescending(func);
                }
                else
                {
                    current = current.OrderBy(func);
                }
            }


            if (string.IsNullOrWhiteSpace(sortName) || string.Equals("HashCode", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Name);
            }
            if (string.Equals("ServerName", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Name);
            }
            else if (string.Equals("Connected", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Connected);
            }
            else if (string.Equals("MaxConnections", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.MaxConnections);
            }
            else if (string.Equals("Season", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Season.Value);
            }
            else if (string.Equals("Mode", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Mode.Value);
            }
            else if (string.Equals("IP", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Address.IP);
            }
            else if (string.Equals("Intent", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Intent.Value);
            }
            else if (string.Equals("Port", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.Port);
            }
            else if (string.Equals("Days", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.DaysInfo?.Day ?? 0);
            }
            else if (string.Equals("DaysInSeason", sortName, StringComparison.OrdinalIgnoreCase))
            {
                HandleOrder(v => v.DaysInfo?.DaysElapsedInSeason ?? 0);
            }
        }

    }

    private void HandleServerName()
    {
        if (queryParams.ServerName is null)
            return;

        var serverName = queryParams.ServerName.Value;

        if (string.IsNullOrWhiteSpace(serverName.Value))
            return;


        if (serverName.IsRegex)
        {
            Regex regex = new(serverName.Value);
            current = current.Where(v => serverName.IsExclude ^ regex.IsMatch(v.Name));
        }
        else
        {
            current = current.Where(v => serverName.IsExclude ^ v.Name.Contains(serverName.Value, StringComparison));
        }
    }

    private void HandlePlayerName()
    {
        if (queryParams.PlayerName is null)
            return;

        var playerName = queryParams.PlayerName.Value;

        if (string.IsNullOrEmpty(playerName.Value))
            return;

        if (playerName.IsRegex)
        {
            Regex regex = CreateRegex(playerName.Value);
            current = current.Where(v => playerName.IsExclude ^ (v.Players is not null && v.Players.Any(p => regex.IsMatch(p.Name))));
        }
        else
        {
            current = current.Where(v => playerName.IsExclude ^ (v.Players is not null && v.Players.Any(p => p.Name.Contains(playerName.Value))));
        }
    }

    private void HandleSeason()
    {
        if (queryParams.Season is null)
            return;

        var season = queryParams.Season.Value;

        if (season.Value is null)
        {
            current = current.Where(v => season.IsExclude ^ v.Season.Value is null);
            return;
        }

        if (season.Value.Length == 0)
            return;

        current = current.Where(v => season.IsExclude ^ season.Value.Any(s => string.Equals(s, v.Season.Value, StringComparison)));
    }

    private void HandleVersion()
    {
        if (queryParams.Version is null)
            return;

        string versionString = queryParams.Version;

        if (versionString is "string")
            return;

        if (string.Equals(versionString, "latest", StringComparison.OrdinalIgnoreCase))
        {
            if (latest == null) return;
            current = current = current.Where(v => v.Version >= latest);
            return;
        }

        Match match = Regex.Match(versionString, @"(?<op>.*?)(?<version>\d+)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'Version' syntax error");
        }

        var op = match.Groups["op"].Value;
        if (!int.TryParse(match.Groups["version"].Value, out var version))
        {
            throw new QueryArgumentException("not a number");
        }

        current = op switch
        {
            "=" or "==" or "" => current.Where(v => v.Version == version),
            ">" => current.Where(v => v.Version > version),
            "<" => current.Where(v => v.Version < version),
            ">=" => current.Where(v => v.Version >= version),
            "<=" => current.Where(v => v.Version <= version),
            _ => throw new QueryArgumentException("'Version' syntax error"),
        };
    }

    private void HandleGameMode()
    {
        if (queryParams.GameMode is null)
            return;

        var gamemode = queryParams.GameMode.Value;

        if (gamemode.Value is null)
        {
            current = current.Where(v => gamemode.IsExclude ^ v.Mode.Value is null);
            return;
        }

        if (gamemode.Value.Length == 0)
            return;

        current = current.Where(v => gamemode.IsExclude ^ gamemode.Value.Any(s => string.Equals(s, v.Mode.Value, StringComparison)));
    }

    private void HandleIntent()
    {
        if (queryParams.Intent is null)
            return;

        var intent = queryParams.Intent.Value;

        if (intent.Value is null)
        {
            current = current.Where(v => intent.IsExclude ^ v.Intent.Value is null);
            return;
        }

        if (intent.Value.Length == 0)
            return;

        current = current.Where(v => intent.IsExclude ^ intent.Value.Any(s => string.Equals(s, v.Intent.Value, StringComparison)));
    }

    private void HandleIP()
    {
        if (queryParams.IP is null)
            return;

        if (queryParams.IP is "string")
            return;

        if (IPAddress.TryParse(queryParams.IP, out var ipAddress))
        {
            current = current.Where(v => v.Address.IP == queryParams.IP);
            return;
        }

        Match match = Regex.Match(queryParams.IP, @"(?<a>(\d+|\*))\.(?<b>\d+|\*)\.(?<c>\d+|\*)\.(?<d>\d+|\*)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'IP' syntax error");
        }

        string[] ips = [match.Groups["a"].Value, match.Groups["b"].Value, match.Groups["c"].Value, match.Groups["d"].Value];

        current = current.Where(v =>
        {
            var split = v.Address.IP.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if (ips[i] == "*")
                {
                    continue;
                }
                else
                {
                    if (ips[i] != split[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        });
    }

    private void HandleHost()
    {
        if (queryParams.Host is null)
            return;

        current = current.Where(v => string.Equals(v.Host, queryParams.Host, StringComparison));
    }

    private void HandlePlatform()
    {
        if (queryParams.Platform is null)
            return;

        var platformString = queryParams.Platform.Value;

        if (platformString.Value is null)
            return;

        if (platformString.Value.Length == 0)
            return;

        if (platformString.Value is ["string"])
            return;

        Platform[] platforms = platformString.Value.Select(v =>
        {
            if (string.Equals("Steam", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.Steam;
            }
            else if (string.Equals("PlayStation", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.PlayStation;
            }
            else if (string.Equals("WeGame", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.WeGame;
            }
            else if (string.Equals("QQGame", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.QQGame;
            }
            else if (string.Equals("Xbox", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.Xbox;
            }
            else if (string.Equals("Switch", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.Switch;
            }
            else if (string.Equals("PS4Official", v, StringComparison.OrdinalIgnoreCase))
            {
                return Platform.PS4Official;
            }
            if (int.TryParse(v, out int platform))
            {
                return (Platform)platform;
            }
            throw new QueryArgumentException("'platform' syntax error");
        }).ToArray();

        current = current.Where(v => platformString.IsExclude ^ platforms.Any(s => v.Platform == s));
    }

    private void HandleCountry()
    {
        if (queryParams.Country is null)
            return;

        var country = queryParams.Country.Value;

        if (country.Value is null)
        {
            current = current.Where(v => country.IsExclude ^ v.Address.IsoCode is null);
            return;
        }

        if (country.Value.Length == 0)
            return;

        current = current.Where(v => country.IsExclude ^ country.Value.Any(s => string.Equals(s, v.Address.IsoCode, StringComparison.OrdinalIgnoreCase)));
    }

    private void HandleModsName()
    {
        if (queryParams.ModsName is null)
            return;

        var modName = queryParams.ModsName.Value;

        if (modName.Value is null)
        {
            return;
        }

        if (modName.Value.Length == 0)
            return;

        if (modName.IsRegex)
        {
            Regex regex = CreateRegex(modName.Value);
            current = current.Where(v => modName.IsExclude ^ (v.ModsInfo?.Any(m => regex.IsMatch(m.Name)) == true));
        }
        else
        {
            current = current.Where(v => modName.IsExclude ^ (v.ModsInfo?.Any(m => m.Name.Contains(modName.Value, StringComparison)) == true));
        }
        var test = current.ToArray();
    }

    private void HandleModsId()
    {
        if (queryParams.ModsId is null)
            return;

        var modsId = queryParams.ModsId.Value;

        if (modsId.Value is null)
            return;

        if (modsId.Value.Length == 0)
            return;

        if (modsId.Value is ["string"])
            return;

        long[] modsIds;
        try
        {
            modsIds = modsId.Value.Select(v => long.Parse(v ?? throw new())).ToArray();
        }
        catch (Exception)
        {
            throw new QueryArgumentException("'ModsId' format error");
        }

        current = current.Where(v => modsId.IsExclude ^ modsIds.Any(id => v.ModsInfo?.Any(m => m.Id == id) == true));
    }

    private void HandleIsPassword()
    {
        if (queryParams.IsPassword is null)
            return;

        current = current.Where(v => v.IsPassword == queryParams.IsPassword.Value);
    }

    private void HandleIsOfficial()
    {
        if (queryParams.IsOfficial is null)
            return;

        current = current.Where(v => v.IsKleiOfficial == queryParams.IsOfficial.Value);
    }

    private void HandleIsPvp()
    {
        if (queryParams.IsPvp is null)
            return;

        current = current.Where(v => v.IsPvp == queryParams.IsPvp.Value);
    }

    private void HandleDays()
    {
        if (queryParams.Days is null)
            return;

        string daysString = queryParams.Days;

        if (daysString is "string")
            return;

        Match match = Regex.Match(daysString, @"(?<op>.*?)(?<days>\d+)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'Days' syntax error");
        }

        var op = match.Groups["op"].Value;
        if (!int.TryParse(match.Groups["days"].Value, out var day))
        {
            throw new QueryArgumentException("not a number");
        }

        current = op switch
        {
            "=" or "==" or "" => current.Where(v => v.DaysInfo?.Day == day),
            ">" => current.Where(v => v.DaysInfo?.Day > day),
            "<" => current.Where(v => v.DaysInfo?.Day < day),
            ">=" => current.Where(v => v.DaysInfo?.Day >= day),
            "<=" => current.Where(v => v.DaysInfo?.Day <= day),
            _ => throw new QueryArgumentException("'Days' syntax error"),
        };
    }

    private void HandleDaysInSeason()
    {
        if (queryParams.DaysInSeason is null)
            return;

        string daysInSeasonString = queryParams.DaysInSeason;

        if (daysInSeasonString is "string")
            return;

        Match match = Regex.Match(daysInSeasonString, @"(?<op>.*?)(?<daysInSeason>\d+)(?<percent>%?)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'DaysInSeason' syntax error");
        }

        var op = match.Groups["op"].Value;
        bool percent = match.Groups["percent"].Value == "%";
        if (!int.TryParse(match.Groups["daysInSeason"].Value, out var daysInSeason))
        {
            throw new QueryArgumentException("not a number");
        }

        Func<LobbyDaysInfo?, int?> func = daysInfo =>
        {
            var r = percent ? (int?)((double?)daysInfo?.DaysElapsedInSeason / daysInfo?.TotalDaysSeason * 100) : daysInfo?.DaysElapsedInSeason;
            return r;
        };
        current = op switch
        {
            "" or "==" or "" => current.Where(v => func(v.DaysInfo) == daysInSeason),
            ">" => current.Where(v => func(v.DaysInfo) > daysInSeason),
            ">=" => current.Where(v => func(v.DaysInfo) >= daysInSeason),
            "<" => current.Where(v => func(v.DaysInfo) < daysInSeason),
            "<=" => current.Where(v => func(v.DaysInfo) <= daysInSeason),
            _ => throw new QueryArgumentException("'DaysInSeason' syntax error"),
        };
    }

    private void HandleDescription()
    {
        if (queryParams.Description is null)
            return;

        var description = queryParams.Description.Value;

        if (description.Value is null)
        {
            current = current.Where(v => v.Description == null);
            return;
        }

        if (description.IsRegex)
        {
            Regex regex = new(description.Value);
            current = current.Where(v => description.IsExclude ^ (v.Description is not null && regex.IsMatch(v.Description)));
        }
        else
        {
            current = current.Where(v => description.IsExclude ^ (v.Description?.Contains(description.Value, StringComparison) == true));
        }
    }

    private void HandleTags()
    {
        if (queryParams.Tags is null)
            return;

        var tags = queryParams.Tags.Value;

        if (tags.Value is null)
        {
            return;
        }

        if (tags.Value.Length == 0)
            return;

        current = current.Where(v => tags.IsExclude ^ tags.Value.Any(tag => v.Tags?.Any(v => tag is not null && v.Contains(tag, StringComparison.OrdinalIgnoreCase)) == true));
    }

    private void HandleMaxConnection()
    {
        if (queryParams.MaxConnections is null)
            return;

        string maxConnectionString = queryParams.MaxConnections;

        if (maxConnectionString is "string")
            return;

        Match match = Regex.Match(maxConnectionString, @"(?<op>.*?)(?<maxConnections>\d+)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'MaxConnections' syntax error");
        }

        var op = match.Groups["op"].Value;
        if (!int.TryParse(match.Groups["maxConnections"].Value, out var maxConnections))
        {
            throw new QueryArgumentException("not a number");
        }

        current = op switch
        {
            "=" or "==" or "" => current.Where(v => v.MaxConnections == maxConnections),
            ">" => current.Where(v => v.MaxConnections > maxConnections),
            "<" => current.Where(v => v.MaxConnections < maxConnections),
            ">=" => current.Where(v => v.MaxConnections >= maxConnections),
            "<=" => current.Where(v => v.MaxConnections <= maxConnections),
            _ => throw new QueryArgumentException("'MaxConnections' syntax error"),
        };
    }

    private void HandleConnected()
    {
        if (queryParams.Connected is null)
            return;

        string connectedString = queryParams.Connected;

        if (connectedString is "string")
            return;

        Match match = Regex.Match(connectedString, @"(?<op>.*?)(?<connected>\d+)(?<percent>%?)");
        if (!match.Success)
        {
            throw new QueryArgumentException("'Connected' syntax error");
        }

        var op = match.Groups["op"].Value;
        bool percent = match.Groups["percent"].Value == "%";
        if (!int.TryParse(match.Groups["connected"].Value, out var connected))
        {
            throw new QueryArgumentException("not a number");
        }

        Func<int, int, int?> func = (cur, max) =>
        {
            return percent ? (int)((double)cur / max * 100) : cur;
        };
        current = op switch
        {
            "" or "==" or "" => current.Where(v => func(v.Connected, v.MaxConnections) == connected),
            ">" => current.Where(v => func(v.Connected, v.MaxConnections) > connected),
            ">=" => current.Where(v => func(v.Connected, v.MaxConnections) >= connected),
            "<" => current.Where(v => func(v.Connected, v.MaxConnections) < connected),
            "<=" => current.Where(v => func(v.Connected, v.MaxConnections) <= connected),
            _ => throw new QueryArgumentException("'Connected' syntax error"),
        };
    }

    private void HandlePlayerPrefab()
    {
        if (queryParams.PlayerPrefab is null)
            return;

        var playerPrefab = queryParams.PlayerPrefab.Value;

        if (string.IsNullOrEmpty(playerPrefab.Value))
            return;

        if (playerPrefab.IsRegex)
        {
            Regex regex = CreateRegex(playerPrefab.Value);
            current = current.Where(v => playerPrefab.IsExclude ^ (v.Players is not null && v.Players.Any(p => regex.IsMatch(p.Prefab))));
        }
        else
        {
            current = current.Where(v => playerPrefab.IsExclude ^ v.Players?.Any(p => p.Prefab.Contains(playerPrefab.Value, StringComparison)) == true);
        }
    }

    private Regex CreateRegex(string pattern)
    {
        try
        {
            return new Regex(pattern, RegexOptions, TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            throw new QueryArgumentException(ex.Message);
        }
    }
}

/// <summary>
/// 查询参数异常
/// </summary>
/// <param name="message"></param>
public class QueryArgumentException(string message) : Exception(message);

/// <summary>
/// 查询参数
/// </summary>
public class QueryParams
{
    /// <summary>
    /// 每页数量
    /// </summary>
    public int? PageCount { get; set; } = 100;
    /// <summary>
    /// 页索引
    /// </summary>
    public int? PageIndex { get; set; }

    /// <summary>
    /// 是否忽略大小写
    /// </summary>
    public bool? IgnoreCase { get; set; } = true;

    /// <summary>
    /// 是否获取详细信息
    /// </summary>
    public bool? IsDetailed { get; set; } = false;


    /// <summary>
    /// 排序, 默认根据字符串HashCode升序排序<br/>
    /// 可以使用|分割,进行多个排序, 使用+-前缀代表升序或者降序排序<br/>
    /// IsExclude属性无效
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Sort { get; set; }

    /// <summary>
    /// 服务器名
    /// </summary>
    [JsonConverter(typeof(RegexValueJsonConverter))]
    public RegexValue? ServerName { get; set; }

    /// <summary>
    /// 玩家名
    /// </summary>
    [JsonConverter(typeof(RegexValueJsonConverter))]
    public RegexValue? PlayerName { get; set; }

    /// <summary>
    /// 季节, 可以使用|获取多个季节
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Season { get; set; }

    /// <summary>
    /// 服务器版本 可以使用运算符&lt; &lt;= > >= =
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 游戏模式, 可以使用|获取多个模式
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? GameMode { get; set; }


    /// <summary>
    /// 游戏风格, 可以使用|获取多个风格
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Intent { get; set; }

    /// <summary>
    /// IP地址, 可以使用通配符\*.\*.\*.\*
    /// </summary>
    public string? IP { get; set; }

    /// <summary>
    /// 房主的KleiId
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 平台, 可以使用|来获取多个平台
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Platform { get; set; }

    /// <summary>
    /// 国家, 根据IsoCode
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Country { get; set; }

    /// <summary>
    /// 根据Mod名搜索
    /// </summary>
    [JsonConverter(typeof(RegexValueJsonConverter))]
    public RegexValue? ModsName { get; set; }

    /// <summary>
    /// 通过ModId搜索Mod, 可以使用|来获取多个Id
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? ModsId { get; set; }

    /// <summary>
    /// 根据天数信息查询, 可以使用运算符&lt; &lt;= > >= =
    /// </summary>
    public string? Days { get; set; }

    /// <summary>
    /// 季节已过去的天数, 可以运算符&lt; &lt;= > >= =或使用%后缀来表达已过去的百分比
    /// </summary>
    public string? DaysInSeason { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonConverter(typeof(RegexValueJsonConverter))]
    public RegexValue? Description { get; set; }

    /// <summary>
    /// 是否有密码
    /// </summary>
    public bool? IsPassword { get; set; }

    /// <summary>
    /// 是否是官方服务器
    /// </summary>
    public bool? IsOfficial { get; set; }

    /// <summary>
    /// 标签, 使用|获取多个标签
    /// </summary>
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public StringArray? Tags { get; set; }

    /// <summary>
    /// 是否PVP
    /// </summary>
    public bool? IsPvp { get; set; }

    /// <summary>
    /// 连接个数, 可以使用%, 或者&lt; &lt;= > >= =
    /// </summary>
    [JsonConverter(typeof(ToStringJsonConverter))]
    public string? Connected { get; set; }

    /// <summary>
    /// 最大连接个数
    /// </summary>
    public string? MaxConnections { get; set; }

    /// <summary>
    /// 玩家角色
    /// </summary>
    [JsonConverter(typeof(RegexValueJsonConverter))]
    public RegexValue? PlayerPrefab { get; set; }
}

/// <summary>
/// 正则值
/// </summary>
public readonly record struct RegexValue
{
    /// <summary>
    /// 值
    /// </summary>
    public string? Value { get; init; }
    /// <summary>
    /// 是否使用正则
    /// </summary>
    public bool IsRegex { get; init; }
    /// <summary>
    /// 是否排除
    /// </summary>
    public bool IsExclude { get; init; }
}

/// <summary>
/// 字符串或数组
/// </summary>
public readonly record struct StringArray
{
    /// <summary>
    /// 值
    /// </summary>
    [JsonConverter(typeof(StringSplitConverter))]
    public string?[]? Value { get; init; }

    /// <summary>
    /// 是否排除
    /// </summary>
    public bool IsExclude { get; init; }
}
