using System.Text.Json.Serialization;
using SteamDownloader.WebApi.Interfaces;

namespace SteamDownloader.WebApi;

public class WorkshopFileDetails
{
    /// <summary>
    /// 1 成功<br/>
    /// </summary>
    [JsonPropertyName("result")]
    public uint Result { get; set; }

    [JsonPropertyName("publishedfileid")]
    public ulong PublishedFileId { get; set; }

    [JsonPropertyName("creator")]
    public ulong Creator { get; set; }

    [JsonPropertyName("creator_appid")]
    public uint CreatorAppid { get; set; }

    [JsonPropertyName("consumer_appid")]
    public uint ConsumerAppid { get; set; }

    [JsonPropertyName("consumer_shortcutid")]
    public uint ConsumerShortcutid { get; set; }

    [JsonPropertyName("filename")]
    public string? FileName { get; set; }

    [JsonPropertyName("file_size")]
    public ulong FileSize { get; set; }

    [JsonPropertyName("preview_file_size")]
    public ulong PreviewFileSize { get; set; }

    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("youtubevideoid")]
    public string? YoutubeVideoId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("hcontent_file")]
    public ulong HContentFile { get; set; }

    [JsonPropertyName("hcontent_preview")]
    public ulong HContentPreview { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("file_description")]
    public string? FileDescription { get; set; }

    [JsonPropertyName("time_created")]
    public DateTimeOffset TimeCreated { get; set; }

    [JsonPropertyName("time_updated")]
    public DateTimeOffset TimeUpdated { get; set; }

    [JsonPropertyName("visibility")]
    public uint Visibility { get; set; }

    [JsonPropertyName("flags")]
    public uint Flags { get; set; }

    [JsonPropertyName("workshop_file")]
    public bool WorkshopFile { get; set; }

    [JsonPropertyName("workshop_accepted")]
    public bool WorkshopAccepted { get; set; }

    [JsonPropertyName("show_subscribe_all")]
    public bool ShowSubscribeAll { get; set; }

    [JsonPropertyName("num_comments_public")]
    public int NumCommentsPublic { get; set; }

    [JsonPropertyName("banned")]
    public bool Banned { get; set; }

    [JsonPropertyName("ban_reason")]
    public string? BanReason { get; set; }

    [JsonPropertyName("banner")]
    public ulong Banner { get; set; }

    [JsonPropertyName("can_be_deleted")]
    public bool CanBeDeleted { get; set; }

    [JsonPropertyName("app_name")]
    public string? AppName { get; set; }

    [JsonPropertyName("file_type")]
    public uint FileType { get; set; }

    [JsonPropertyName("can_subscribe")]
    public bool CanSubscribe { get; set; }

    [JsonPropertyName("subscriptions")]
    public uint Subscriptions { get; set; }

    [JsonPropertyName("favorited")]
    public uint Favorited { get; set; }

    [JsonPropertyName("followers")]
    public uint Followers { get; set; }

    [JsonPropertyName("lifetime_subscriptions")]
    public uint LifetimeSubscriptions { get; set; }

    [JsonPropertyName("lifetime_favorited")]
    public uint LifetimeFavorited { get; set; }

    [JsonPropertyName("lifetime_followers")]
    public uint LifetimeFollowers { get; set; }

    [JsonPropertyName("lifetime_playtime")]
    public ulong LifetimePlaytime { get; set; }

    [JsonPropertyName("lifetime_playtime_sessions")]
    public ulong LifetimePlaytimeSessions { get; set; }

    [JsonPropertyName("views")]
    public uint Views { get; set; }

    [JsonPropertyName("num_children")]
    public uint NumChildren { get; set; }

    [JsonPropertyName("num_reports")]
    public uint NumReports { get; set; }

    [JsonPropertyName("tags")]
    public WorkshopTag[]? Tags { get; set; }

    [JsonPropertyName("language")]
    public PublishedFileServiceLanguage Language { get; set; }

    [JsonPropertyName("maybe_inappropriate_sex")]
    public bool MaybeInappropriateSex { get; set; }

    [JsonPropertyName("maybe_inappropriate_violence")]
    public bool MaybeInappropriateViolence { get; set; }

    [JsonPropertyName("revision_change_number")]
    public ulong RevisionChangeNumber { get; set; }

    [JsonPropertyName("revision")]
    public SteamKit2.Internal.EPublishedFileRevision Revision { get; set; }

    [JsonPropertyName("ban_text_check_result")]
    public SteamKit2.Internal.EBanContentCheckResult BanTextCheckResult { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("previews")]
    public WorkshopPreview[]? Previews { get; set; }

    [JsonPropertyName("vote_data")]
    public WorkshopVoteData? VoteData { get; set; }

    public record WorkshopVoteData
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("votes_up")]
        public int VotesUp { get; set; }

        [JsonPropertyName("votes_down")]
        public int VotesDown { get; set; }
    }

    public record WorkshopPreview
    {
        [JsonPropertyName("previewid")]
        public ulong PrewviewId { get; set; }

        [JsonPropertyName("sortorder")]
        public uint SortOrder { get; set; }

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("filename")]
        public string? FileName { get; set; }
        [JsonPropertyName("preview_type")]

        public uint PreviewType { get; set; }

        public WorkshopPreview() { }

        public WorkshopPreview(SteamKit2.Internal.PublishedFileDetails.Preview preview)
        {
            PrewviewId = preview.previewid;
            SortOrder = preview.sortorder;
            Size = preview.size;
            FileName = preview.filename;
            PreviewType = preview.preview_type;
            try
            {
                Url = new Uri(preview.url);
            }
            catch (Exception) { }
        }

    }
}


public static class WorkshopFileDetailsExtensions
{
    public static WorkshopFileDetails ToWorkshopFileDetails(this SteamKit2.Internal.PublishedFileDetails publishedFileDetails)
    {
        WorkshopFileDetails details = new(); 
        details.Result = publishedFileDetails.result;
        details.PublishedFileId = publishedFileDetails.publishedfileid;
        details.Creator = publishedFileDetails.creator;
        details.CreatorAppid = publishedFileDetails.creator_appid;
        details.ConsumerAppid = publishedFileDetails.consumer_appid;
        details.ConsumerShortcutid = publishedFileDetails.consumer_shortcutid;
        details.FileName = publishedFileDetails.filename;
        details.FileSize = publishedFileDetails.file_size;
        details.PreviewFileSize = publishedFileDetails.preview_file_size;
        details.FileUrl = publishedFileDetails.file_url;
        details.PreviewUrl = publishedFileDetails.preview_url;
        details.YoutubeVideoId = publishedFileDetails.youtubevideoid;
        details.Url = publishedFileDetails.url;
        details.HContentFile = publishedFileDetails.hcontent_file;
        details.HContentPreview = publishedFileDetails.hcontent_preview;
        details.Title = publishedFileDetails.title;
        details.FileDescription = publishedFileDetails.file_description;
        details.TimeCreated = DateTimeOffset.FromUnixTimeSeconds(publishedFileDetails.time_created);
        details.TimeUpdated = DateTimeOffset.FromUnixTimeSeconds(publishedFileDetails.time_updated);
        details.Visibility = publishedFileDetails.visibility;
        details.Flags = publishedFileDetails.flags;
        details.WorkshopFile = publishedFileDetails.workshop_file;
        details.WorkshopAccepted = publishedFileDetails.workshop_accepted;
        details.ShowSubscribeAll = publishedFileDetails.show_subscribe_all;
        details.NumCommentsPublic = publishedFileDetails.num_comments_public;
        details.Banned = publishedFileDetails.banned;
        details.BanReason = publishedFileDetails.ban_reason;
        details.Banner = publishedFileDetails.banner;
        details.CanBeDeleted = publishedFileDetails.can_be_deleted;
        details.AppName = publishedFileDetails.app_name;
        details.FileType = publishedFileDetails.file_type;
        details.CanSubscribe = publishedFileDetails.can_subscribe;
        details.Subscriptions = publishedFileDetails.subscriptions;
        details.Favorited = publishedFileDetails.favorited;
        details.Followers = publishedFileDetails.followers;
        details.LifetimeSubscriptions = publishedFileDetails.lifetime_subscriptions;
        details.LifetimeFavorited = publishedFileDetails.lifetime_favorited;
        details.LifetimeFollowers = publishedFileDetails.lifetime_followers;
        details.LifetimePlaytime = publishedFileDetails.lifetime_playtime;
        details.LifetimePlaytimeSessions = publishedFileDetails.lifetime_playtime_sessions;
        details.Views = publishedFileDetails.views;
        details.NumChildren = publishedFileDetails.num_children;
        details.NumReports = publishedFileDetails.num_reports;
        details.Tags = publishedFileDetails.tags?.Select(v => new WorkshopTag() { Tag = v.tag, DisplayName = v.display_name }).ToArray();
        details.Language = (PublishedFileServiceLanguage)publishedFileDetails.language;
        details.MaybeInappropriateSex = publishedFileDetails.maybe_inappropriate_sex;
        details.MaybeInappropriateViolence = publishedFileDetails.maybe_inappropriate_violence;
        details.RevisionChangeNumber = publishedFileDetails.revision_change_number;
        details.Revision = publishedFileDetails.revision;
        details.BanTextCheckResult = publishedFileDetails.ban_text_check_result;
        details.ShortDescription = publishedFileDetails.short_description;

        return details;
    }
}

