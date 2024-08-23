using System.Text.RegularExpressions;
using DstDownloaders.Mods;
using SteamDownloader.WebApi.Interfaces;

namespace Ilyfairy.DstServerQuery.Web.Models.Http.Mods;

/// <summary>
/// Mods查询参数
/// </summary>
public record QueryModsParams
{
    public string? Text { get; set; } = string.Empty;

    public int? PageIndex { get; set; } = 0;

    public int? PageSize { get; set; } = 100;

    public bool? IsQueryName { get; set; } = true;
    public bool? IsQueryDescription { get; set; } = true;
    public bool? IsQueryAuthor { get; set; } = true;
    public bool? IsQueryTag { get; set; } = true;
    public bool? IsQueryWorkshopId { get; set; } = true;

    public bool? IgnoreCase { get; set; } = true;

    /// <summary>
    /// 排序, +表示升序, -表示降序, 默认升序<br/>
    /// <see cref="SortType"/>
    /// </summary>
    public string? Sort { get; set; }

    public DstModType? Type { get; set; }

    public PublishedFileServiceLanguage? Language { get; set; }

    public enum SortType
    {
        Relevance = default,
        UpdateTime,
        CreatedTime,
        Name,
        WorkshopId,
        Size,
        Views,
        Subscriptions,
        Favorited,
        CommentsPublic,
    }
}



public class WebModsInfoLite
{
    /// <summary>
    /// 创意工坊Id
    /// </summary>
    public ulong WorkshopId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }


    /// <summary>
    /// Mod的更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// Mod的创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }


    /// <summary>
    /// 创建者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 创建者的SteamId
    /// </summary>
    public ulong AuthorSteamId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string[] Tags { get; set; }

    /// <summary>
    /// 服务端Mod还是客户端Mod
    /// </summary>
    public DstModType? ModType { get; set; }

    /// <summary>
    /// 是否是 UGC Mod
    /// </summary>
    public bool IsUGC { get; set; }

    /// <summary>
    /// 最新版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 预览图片链接
    /// </summary>
    public Uri? PreviewImageUrl { get; set; }
    public string? PreviewImageType { get; set; }

    //public bool IsDescriptionMarkup { get; set; }

    public PublishedFileServiceLanguage Language { get; set; }

    public WebModsInfoLite(DstModStore store, PublishedFileServiceLanguage? language = null)
    {
        if (store is null)
        {
            throw new NullReferenceException("store为null");
        }
        if (store.SteamModInfo is null)
        {
            throw new NullReferenceException($"SteamModInfo为null WorkshopId:{store.WorkshopId}");
        }

        WorkshopId = store.WorkshopId;
        Name = store.SteamModInfo!.Name!;
        Description = store.SteamModInfo.Description;

        //获取指定语言的描述
        Language = PublishedFileServiceLanguage.English;
        if (language != null && store.ExtInfo.MultiLanguage?.TryGetValue(language.Value, out var data) is true)
        {
            Name = data.Name ?? Name;
            Description = data.Description;
            Language = language.Value;
        }
        
        UpdateTime = store.UpdatedTime!.Value;
        CreatedTime = store.SteamModInfo.CreatedTime;

        Author = store.ModInfoLua?.Author;
        ModType = store.ModInfoLua?.DstModType;
        Version = store.ModInfoLua?.Version;
        Tags = store.SteamModInfo.Tags ?? [];

        PreviewImageUrl = store.SteamModInfo.PreviewImageUrl;
        PreviewImageType = store.ExtInfo.PreviewImageType;
    }
}

public class ModsQueryResponse : ResponseBase
{
    public IReadOnlyCollection<WebModsInfoLite>? Mods { get; set; }
    public int PageIndex { get; set; }
    public int Count { get; set; }
    public int MaxPageIndex { get; set; }
    public int TotalCount { get; set; }
}
