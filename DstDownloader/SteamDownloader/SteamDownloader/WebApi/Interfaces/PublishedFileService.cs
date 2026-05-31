using System.Text.Json;
using SteamDownloader.Helpers;

namespace SteamDownloader.WebApi.Interfaces;

/// <summary>
/// see https://partner.steamgames.com/doc/webapi/IPublishedFileService
/// </summary>
/// <param name="steamSession"></param>
public class PublishedFileService(SteamSession steamSession) : InterfaceBase(steamSession)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="query_type">查询 UCG 物品的方式</param>
    /// <param name="page">当前页, 第一页是1, 当前上限为 1000</param>
    /// <param name="cursor">
    /// 将光标移至结果页(将第一个请求设置为'*'), 由于此参数允许您进行深度编页, 请优先考虑使用此参数, 而非页面参数<br/>
    /// 一经使用,页面参数将被忽略<br/>
    /// 使用在响应中返回的"next_cursor"值, 设置下一个查询以获得下一组结果
    /// </param>
    /// <param name="numperpage">每页返回的结果的数量</param>
    /// <param name="creator_appid"></param>
    /// <param name="appid"></param>
    /// <param name="requiredtags">要匹配的标签</param>
    /// <param name="excludedtags">绝不能在已发布的文件里出现以满足查询的标签</param>
    /// <param name="match_all_tags">若为 true, 则物品必须有全部指定标签, 否则必须至少有其中一个标签</param>
    /// <param name="required_flags"></param>
    /// <param name="omitted_flags"></param>
    /// <param name="search_text">在物品标题或描述中进行匹配的文字</param>
    /// <param name="filetype"></param>
    /// <param name="child_publishedfileid">查找所有引用给定物品的物品</param>
    /// <param name="days"></param>
    /// <param name="include_recent_votes_only"></param>
    /// <param name="cache_max_age_seconds">允许为指定秒数返回过期数据</param>
    /// <param name="language">要搜索并返回的语言<br/>默认为英语</param>
    /// <param name="required_kv_tags">要匹配所必需的键值标签</param>
    /// <param name="totalonly"></param>
    /// <param name="ids_only"></param>
    /// <param name="return_vote_data"></param>
    /// <param name="return_tags"></param>
    /// <param name="return_kv_tags"></param>
    /// <param name="return_previews"></param>
    /// <param name="return_children"></param>
    /// <param name="return_short_description">填充 short_description 字段, 而非 file_description 字段</param>
    /// <param name="return_for_sale_data"></param>
    /// <param name="return_metadata">填充元数据</param>
    /// <param name="return_playtime_stats"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<QueryFilesResponse> QueryFiles(PublishedFileQueryType? query_type = null,
                                                     uint? page = null,
                                                     string? cursor = null,
                                                     uint? numperpage = null,
                                                     string? creator_appid = null,
                                                     uint? appid = null,
                                                     string? requiredtags = null,
                                                     string? excludedtags = null,
                                                     bool? match_all_tags = null,
                                                     string? required_flags = null,
                                                     string? omitted_flags = null,
                                                     string? search_text = null,
                                                     PublishedFileInfoMatchingFileType? filetype = null,
                                                     ulong? child_publishedfileid = null,
                                                     uint? days = null,
                                                     bool? include_recent_votes_only = null,
                                                     uint? cache_max_age_seconds = null,
                                                     PublishedFileServiceLanguage? language = null,
                                                     object? required_kv_tags = null,
                                                     bool? totalonly = null,
                                                     bool? ids_only = null,
                                                     bool? return_vote_data = null,
                                                     bool? return_tags = null,
                                                     bool? return_kv_tags = null,
                                                     bool? return_previews = null,
                                                     bool? return_children = null,
                                                     bool? return_short_description = null,
                                                     bool? return_for_sale_data = null,
                                                     bool? return_metadata = null,
                                                     uint? return_playtime_stats = null,
                                                     CancellationToken cancellationToken = default
        )
    {
        KeyValuePair<string, string?>[] kvs =
            [
                new("key", ApiKey),
                new(nameof(query_type), ((int?)query_type)?.ToString()),
                new(nameof(page), page?.ToString()),
                new(nameof(cursor), cursor),
                new(nameof(numperpage), numperpage?.ToString()),
                new(nameof(creator_appid), creator_appid),
                new(nameof(appid), appid?.ToString()),
                new(nameof(requiredtags), requiredtags),
                new(nameof(excludedtags), excludedtags),
                new(nameof(match_all_tags), match_all_tags?.ToString()),
                new(nameof(required_flags), required_flags),
                new(nameof(omitted_flags), omitted_flags),
                new(nameof(search_text), search_text),
                new(nameof(filetype), ((int?)filetype)?.ToString()),
                new(nameof(child_publishedfileid), child_publishedfileid?.ToString()),
                new(nameof(days), days?.ToString()),
                new(nameof(include_recent_votes_only), include_recent_votes_only?.ToString()),
                new(nameof(cache_max_age_seconds), cache_max_age_seconds?.ToString()),
                new(nameof(language), ((int)(language ?? PublishedFileServiceLanguage.English)).ToString()),
                new(nameof(required_kv_tags), required_kv_tags?.ToString()),
                new(nameof(totalonly), totalonly?.ToString()),
                new(nameof(ids_only), ids_only?.ToString()),
                new(nameof(return_vote_data), return_vote_data?.ToString()),
                new(nameof(return_tags), return_tags?.ToString()),
                new(nameof(return_kv_tags), return_kv_tags?.ToString()),
                new(nameof(return_previews), return_previews?.ToString()),
                new(nameof(return_children), return_children?.ToString()),
                new(nameof(return_short_description), return_short_description?.ToString()),
                new(nameof(return_for_sale_data), return_for_sale_data?.ToString()),
                new(nameof(return_metadata), return_metadata?.ToString()),
                new(nameof(return_playtime_stats), return_playtime_stats?.ToString()),
            ];

        Uri url = new(WebApiBaseAddress, $"/IPublishedFileService/QueryFiles/v1/?{Utils.MakeQueryParams(kvs)}");
        try
        {
            var stream = await steamSession.HttpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
            WebApiResponse<QueryFilesResponse>? response = JsonSerializer.Deserialize<WebApiResponse<QueryFilesResponse>>(stream, JsonOptions);
            return response?.Response ?? throw new ArgumentNullException("response");
        }
        catch (Exception e)
        {
            throw;
        }
    }
}


public enum PublishedFileServiceLanguage
{
    English = 0,
    Chinese = 6,
}