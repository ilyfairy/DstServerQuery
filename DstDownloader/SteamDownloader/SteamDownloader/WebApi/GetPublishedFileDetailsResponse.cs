using System.Text.Json.Serialization;

namespace SteamDownloader.WebApi;

public class GetPublishedFileDetailsResponse
{
    [JsonPropertyName("result")]
    public uint Result { get; set; }

    [JsonPropertyName("resultcount")]
    public uint ResultCount { get; set; }

    [JsonPropertyName("publishedfiledetails")]
    public WorkshopStorageFileDetails[]? PublishedFileDetails { get; set; }
}


public class WorkshopStorageFileDetails
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

    [JsonPropertyName("creator_app_id")]
    public uint CreatorAppId { get; set; }

    [JsonPropertyName("consumer_app_id")]
    public uint ConsumerAppId { get; set; }

    [JsonPropertyName("filename")]
    public string? FileName { get; set; } // ""

    [JsonPropertyName("file_size")]
    public ulong FileSize { get; set; } // 0

    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; } // ""

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("hcontent_file")]
    public ulong HContentFile { get; set; }

    [JsonPropertyName("hcontent_preview")]
    public ulong HContentPreview { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("time_created")]
    public DateTimeOffset TimeCreated { get; set; }

    [JsonPropertyName("time_updated")]
    public DateTimeOffset TimeUpdated { get; set; }

    [JsonPropertyName("visibility")]
    public uint Visibility { get; set; }

    [JsonPropertyName("banned")]
    public bool Banned { get; set; }
    
    [JsonPropertyName("ban_reason")]
    public string? BanReason { get; set; }

    [JsonPropertyName("can_subscribe")]
    public bool CanSubscribe { get; set; }

    [JsonPropertyName("subscriptions")]
    public uint Subscriptions { get; set; }

    [JsonPropertyName("favorited")]
    public uint Favorited { get; set; }


    [JsonPropertyName("lifetime_subscriptions")]
    public uint LifetimeSubscriptions { get; set; }

    [JsonPropertyName("lifetime_favorited")]
    public uint LifetimeFavorited { get; set; }

    [JsonPropertyName("views")]
    public uint Views { get; set; }

    [JsonPropertyName("tags")]
    public WorkshopStorageFileDetailsTag[]? Tags { get; set; }

    public class WorkshopStorageFileDetailsTag
    {
        [JsonPropertyName("tag")]
        public string? Tag { get; set; }
    }
}
