using System.Text.RegularExpressions;
using Asp.Versioning;
using DstDownloaders.Mods;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Web.Models;
using Ilyfairy.DstServerQuery.Web.Models.Http.Mods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("fixed")]
public class ModsController(
            ILogger<ModsController> logger,
            LobbyServerManager LobbyDetailsManager, 
            DstModsFileService modsService,
            IMemoryCache memoryCache) : ControllerBase
{
    private readonly LobbyServerManager lobbyDetailsManager = LobbyDetailsManager;
    private readonly DstModsFileService modsService = modsService;
    private readonly ILogger _logger = logger;


    /// <summary>
    /// 获取指定平台Mods的信息和使用量
    /// </summary>
    /// <param name="maxCount">最大返回个数</param>
    /// <param name="platform">平台, 默认Steam</param>
    /// <returns></returns>
    [HttpPost("GetUsage")]
    [ProducesResponseType<DstModsUsageResponse>(200)]
    public IActionResult GetUsage(int maxCount = 100, string platform = "Steam")
    {
        //_logger.Info("获取Mod个数 TopCount{0}", maxCount);
        var list = lobbyDetailsManager.GetCurrentServers();

        Platform platformEnum;
        if (int.TryParse(platform, out var platformNum))
        {
            platformEnum = (Platform)platformNum;
        }
        else if (Enum.TryParse(platform, true, out platformEnum)) { }
        else
        {
            platformEnum = Platform.Steam;
        }

        IEnumerable<LobbyModInfo> mods = list.Where(v => v is { ModsInfo: { } } && v.Platform == platformEnum)
            .SelectMany(v => v.ModsInfo ?? []);

        var group = mods.GroupBy(v => v.Id).Select(v =>
        {
            var mod = v.First();
            return new DstModCount(mod.Id, mod.Name, v.Count());
        });

        var r = group.OrderByDescending(v => v.Count);

        DstModsUsageResponse response = new();
        response.Mods = r.Take(maxCount);

        return response.ToJsonResult();
    }


    /// <summary>
    /// 获取正在游玩的Mods的名称和使用量
    /// </summary>
    /// <param name="minUsage">最小个数</param>
    /// <returns></returns>
    [HttpPost("GetUsageNames")]
    [ProducesResponseType<DstModsNameUsageResponse>(200)]
    public IActionResult GetUsageNames([FromQuery] int minUsage = 1)
    {
        var list = lobbyDetailsManager.GetCurrentServers();

        IEnumerable<string> mods = list.SelectMany(v => v.ModsInfo ?? []).Select(v => v.Name);

        var group = mods.GroupBy(v => v).Select(v => new DstModNameCount(v.Key, v.Count()));

        var r = group.OrderByDescending(v => v.Count)
            .Where(v => v.Count >= minUsage);

        DstModsNameUsageResponse response = new();
        response.Mods = r;

        return response.ToJsonResult();
    }


    /// <summary>
    /// 获取Mod信息
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetModsInfo")]
    [ProducesResponseType<DstModsInfoResponse>(200)]
    public async Task<IActionResult> GetModInfo([FromBody] DstModsInfoRequest request)
    {
        var info = await modsService.GetOrDownloadAsync(request.WorkshopId);

        if (info is null)
        {
            return ResponseBase.NotFound("Mod不存在");
        }

        return new DstModsInfoResponse()
        {
            Mod = new WebModsInfo(info, request.Language)
        }.ToJsonResult();
    }

    private readonly char[] SortSplitChars = [',', '|', ';'];
    /// <summary>
    /// 查询Mods
    /// </summary>
    /// <returns></returns>
    [HttpPost("QueryMods")]
    [ProducesResponseType<ModsQueryResponse>(200)]
    public IActionResult QueryMods([FromBody] QueryModsParams param)
    {
        bool isCache = false;
        if (param is
            {
                PageSize: null or 100, Text: null or "", PageIndex: 0 or 1,
                Language: SteamDownloader.WebApi.Interfaces.PublishedFileServiceLanguage.English or SteamDownloader.WebApi.Interfaces.PublishedFileServiceLanguage.Chinese,
            })
        {
            isCache = true;
            if (memoryCache.TryGetValue<ModsQueryResponse>(param, out var value))
            {
                return value!.ToJsonResult();
            }
        }

        if (param.PageSize > 1000)
            param.PageSize = 1000;

        if (param.PageSize is null or < 0)
            param.PageSize = 1;

        if (param.PageIndex is null or < 0)
            param.PageIndex = 0;

        var text = param.Text;

        Regex? regex = null;
        Func<string?, bool> IsMatch;
        if (string.IsNullOrEmpty(text))
        {
            IsMatch = v => true;
        }
        else if (text.Length > 2 && text[0] == '/' && text[^1] == '/') // 如果是/正则/
        {
            try
            {
                regex = new Regex(text[1..^1], param.IgnoreCase is true ? RegexOptions.IgnoreCase : RegexOptions.None);
            }
            catch (Exception)
            {
                return ResponseBase.BadRequest("正则表达式错误");
            }
            IsMatch = v =>
            {
                if (v is null)
                    return false;

                return regex.IsMatch(v);
            };
        }
        else
        {
            IsMatch = v => v?.Contains(text, param.IgnoreCase is true ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) is true;
        }

        //查询过滤器
        QueryFilterResult QueryFunc(DstModStore v)
        {
            if (v == null)
            {
                throw new Exception("QueryFunc.arg1意外null");
            }
            if (string.IsNullOrEmpty(text))
                return new QueryFilterResult(v, true, 0);

            if (v.SteamModInfo is null)
                return new QueryFilterResult(v, false, 0);

            var steamModInfo = v.SteamModInfo;

            if (param.IsQueryWorkshopId is { })
            {
                if (ulong.TryParse(text, out var id) && v.WorkshopId == id)
                    return new QueryFilterResult(v, true, 10);
            }

            if (param.IsQueryName is true
                && (IsMatch(steamModInfo.Name)
                    || v.ExtInfo.MultiLanguage?.Any(v => IsMatch(v.Value.Name)) == true))
                return new QueryFilterResult(v, true, 9);

            if (param.IsQueryDescription is true
                && (IsMatch(steamModInfo.Description)
                || v.ExtInfo.MultiLanguage?.Any(v => IsMatch(v.Value.Description)) == true))
                return new QueryFilterResult(v, true, 8);

            if (param.IsQueryTag is true && steamModInfo.Tags?.Any(v => IsMatch(v)) is true)
                return new QueryFilterResult(v, true, 7);

            if (param.IsQueryAuthor is true && IsMatch(v.ModInfoLua?.Author))
                return new QueryFilterResult(v, true, 6);

            if (param.Type is DstModType.Client or DstModType.Server && v.ModInfoLua?.DstModType == param.Type)
                return new QueryFilterResult(v, true, 0);

            return new QueryFilterResult(v, false, 0);
        }

        var result = modsService.Cache
            .AsParallel()
            .Where(v => v != null)
            .Select(QueryFunc!) // 抑制参数的可空警告
            .Where(v => v.IsOk)
            .Select(v =>
            {
                if (v.Store == null)
                {
                    throw new Exception("Store意外null");
                }
                v.WebModsInfo = new WebModsInfo(v.Store, param.Language);
                return v;
            })
            .AsEnumerable();

        //排序
        if (string.IsNullOrWhiteSpace(param.Sort))
        {
            if (string.IsNullOrEmpty(text))
            {
                result = result.OrderByDescending(v => v.WebModsInfo!.View.Views); // 访问量排序
            }
            else
            {
                result = result.OrderByDescending(v => v.Level); // 相关程度排序
            }
        }
        else
        {
            var sorts = param.Sort.Split(SortSplitChars).Take(10);
            foreach (var sort in sorts)
            {
                QueryModsParams.SortType sortType = QueryModsParams.SortType.Relevance;
                bool isDesc = false;
                if (sort[0] is '+' or '-')
                {
                    isDesc = sort[0] == '-';
                    _ = Enum.TryParse(sort.AsSpan(1), out sortType);
                }
                else
                {
                    _ = Enum.TryParse(sort, out sortType);
                }

                IEnumerable<QueryFilterResult> SortSelector<T>(Func<QueryFilterResult, T> func)
                    => isDesc ? result.OrderByDescending(func) : result.OrderBy(func);

                result = sortType switch
                {
                    QueryModsParams.SortType.Relevance => result.OrderByDescending(v => v.Level),
                    QueryModsParams.SortType.UpdateTime => SortSelector(v => v.WebModsInfo!.UpdateTime),
                    QueryModsParams.SortType.CreatedTime => SortSelector(v => v.WebModsInfo!.CreatedTime),
                    QueryModsParams.SortType.Name => SortSelector(v => v.WebModsInfo!.Name),
                    QueryModsParams.SortType.WorkshopId => SortSelector(v => v.WebModsInfo!.WorkshopId),
                    QueryModsParams.SortType.Size => SortSelector(v => v.WebModsInfo!.Size),
                    QueryModsParams.SortType.Views => SortSelector(v => v.WebModsInfo!.View.Views),
                    QueryModsParams.SortType.Subscriptions => SortSelector(v => v.WebModsInfo!.View.Subscriptions),
                    QueryModsParams.SortType.Favorited => SortSelector(v => v.WebModsInfo!.View.Favorited),
                    QueryModsParams.SortType.CommentsPublic => SortSelector(v => v.WebModsInfo!.View.CommentsPublic),
                    _ => result,
                };

            }
        }
        
        List<WebModsInfoLite> list = new();
        var or = result.GetEnumerator();
        int count = 0;
        var curSkip = 0;
        var skip = param.PageIndex * param.PageSize;
        while (or.MoveNext())
        {
            count++;
            if (curSkip < skip)
            {
                curSkip++;
                continue;
            }
            if (list.Count < param.PageSize)
            {
                list.Add(or.Current.WebModsInfo!);
            }
        }
        var maxIndex = count / param.PageSize; // (int)MathF.Ceiling((float));
        ModsQueryResponse response = new()
        {
            Mods = list,
            PageIndex = param.PageIndex.Value,
            Count = list.Count,
            TotalCount = count,
            MaxPageIndex = maxIndex.Value,
        };

        if (isCache)
        {
            memoryCache.Set(param, response, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                Size = 10,
            });
            _logger.LogInformation("QueryMods已缓存 {Params}", param);
        }

        return response.ToJsonResult();
    }


}


file class QueryFilterResult(DstModStore store, bool isOk, int level)
{
    public DstModStore Store { get; set; } = store;

    public bool IsOk { get; set; } = isOk;

    /// <summary>
    /// 搜索相关程度(数字越高则越高)
    /// </summary>
    public int Level { get; set; } = level;

    public WebModsInfo? WebModsInfo { get; set; } = null;
}