using System.Text.Json.Serialization;
using DstDownloaders.Converters;
using SteamDownloader.WebApi;

namespace DstDownloaders.Mods;

[JsonConverter(typeof(SteamModInfoJsonConverter))]
public class SteamModInfo
{
    public readonly WorkshopFileDetails details;
    public SteamModInfo(WorkshopFileDetails details)
    {
        this.details = details;
        OptimizeString();

        Tags = details.Tags?.Select(v => v.Tag).ToArray() ?? [];
    }

    private void OptimizeString()
    {
        if (details.AppName is "Don't Starve Together")
            details.AppName = "Don't Starve Together";

        if (details.FileName is "mod_publish_data_file.zip")
            details.FileName = "mod_publish_data_file.zip";

        foreach (var item in details.Tags ?? [])
        {
            if (item.Tag is "all_clients_require_mod")
            {
                item.Tag = "all_clients_require_mod";
                item.DisplayName = "all_clients_require_mod";
            }
            else if (item.Tag is "character")
            {
                item.Tag = "character";
                item.DisplayName = "character";
            }
            else if (item.Tag is "item")
            {
                item.Tag = "item";
                item.DisplayName = "item";
            }
            else if (item.Tag is "language")
            {
                item.Tag = "language";
                item.DisplayName = "language";
            }
            else if (item.Tag is "client_only_mod")
            {
                item.Tag = "client_only_mod";
                item.DisplayName = "client_only_mod";
            }
        }

        if (details.ShortDescription == details.FileDescription)
        {
            details.ShortDescription = details.FileDescription;
        }
    }

    public bool IsValid
    {
        get
        {
            if (details.Result == 1)
                return true;
            if (details.Result is 8 or 0 or 9 or 15)
                return false;

            return false;
        }
    }

    /// <summary>
    /// Mod名称
    /// </summary>
    public string? Name => details.Title;

    /// <summary>
    /// 是否可以订阅
    /// </summary>
    public bool CanSubscribe => details.CanSubscribe;

    /// <summary>
    /// 是否是UGC的Mod
    /// </summary>
    public bool IsUGC => string.IsNullOrEmpty(details.FileUrl) && IsValid;

    /// <summary>
    /// Mod描述
    /// </summary>
    public string? Description => details.FileDescription is null ? details.ShortDescription : details.FileDescription;

    /// <summary>
    /// Mod ID
    /// </summary>
    public ulong WorkshopId => details.PublishedFileId;

    /// <summary>
    /// Tags
    /// </summary>
    public string[]? Tags { get; }

    /// <summary>
    /// 文件链接
    /// </summary>
    public Uri? FileUrl => string.IsNullOrWhiteSpace(details.FileUrl) ? null : new Uri(details.FileUrl);

    /// <summary>
    /// Mod文件大小
    /// </summary>
    public ulong FileSize => details.FileSize;

    /// <summary>
    /// 预览图片Url
    /// </summary>
    public Uri? PreviewImageUrl => string.IsNullOrWhiteSpace(details.PreviewUrl) ? null : new Uri(details.PreviewUrl);

    /// <summary>
    /// 预览图片大小
    /// </summary>
    public ulong PreviewImageSize => details.PreviewFileSize;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime => details.TimeCreated;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedTime => details.TimeUpdated;

    /// <summary>
    /// 创建者的SteamID
    /// </summary>
    public ulong CreatorSteamId => details.Creator;




    /// <summary>
    /// 不重复访客次数
    /// </summary>
    public ulong Views => details.Views;

    /// <summary>
    /// 当前订阅人数
    /// </summary>
    public uint Subscriptions => details.Subscriptions;

    /// <summary>
    /// 当前收藏人数
    /// </summary>
    public long Favorited => details.Favorited;

    /// <summary>
    /// 公开的评论数量
    /// </summary>
    public int CommentsPublic => details.NumCommentsPublic;

}
