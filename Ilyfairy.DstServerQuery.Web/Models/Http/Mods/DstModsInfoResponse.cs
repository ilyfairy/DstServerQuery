using DstDownloaders.Mods;
using SteamDownloader.WebApi;
using SteamDownloader.WebApi.Interfaces;

namespace Ilyfairy.DstServerQuery.Web.Models.Http.Mods;


public class DstModsInfoRequest
{
    public ulong WorkshopId { get; set; }
    public PublishedFileServiceLanguage? Language { get; set; }
}



public class DstModsInfoResponse : ResponseBase
{
    /// <summary>
    /// Mod信息
    /// </summary>
    public required WebModsInfo Mod { get; set; }
}


/// <summary>
/// Mod信息
/// </summary>
public class WebModsInfo : WebModsInfoLite
{
    /// <summary>
    /// Mod占用大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 浏览信息
    /// </summary>
    public SteamModsView View { get; set; }

    /// <summary>
    /// 配置选项
    /// </summary>
    public DstConfigurationOption[]? ConfigurationOptions { get; set; }

    public WorkshopPreview[]? Previews { get; set; }

    public SteamVoteData? VoteData { get; set; }

    public WebModsInfo(DstModStore store, PublishedFileServiceLanguage? language) : base(store, language)
    {
        AuthorSteamId = store.SteamModInfo!.CreatorSteamId;
        IsUGC = store.SteamModInfo.IsUGC;
        Size = store.ExtInfo.Size;
        View = new((int)store.SteamModInfo.Views,
            (int)store.SteamModInfo.Subscriptions,
            (int)store.SteamModInfo.Favorited,
            store.SteamModInfo.CommentsPublic);

        ConfigurationOptions = store.ModInfoLua?.ConfigurationOptions;

        if (store.SteamModInfo.details.VoteData is { } voteData)
            VoteData = new(voteData.Score, voteData.VotesUp, voteData.VotesDown);
        if(store.SteamModInfo.details.Previews is { } previews)
            Previews = previews.Select(v => new WorkshopPreview(v.PrewviewId, v.SortOrder, v.Url, v.Size, v.FileName, v.PreviewType)).ToArray();
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="Views">不重复访客数</param>
/// <param name="Subscriptions">当前订阅者</param>
/// <param name="Favorited">当前收藏人数</param>
/// <param name="CommentsPublic">公开评论数量</param>
public record SteamModsView(int Views, int Subscriptions, int Favorited, int CommentsPublic);

public record WorkshopPreview(ulong PrewviewId, uint SortOrder, Uri? Url, long Size, string? FileName, uint PreviewType);

public record SteamVoteData(double Score, int VotesUp, int VotesDown);
