using System.Text.Json.Serialization;

namespace SteamDownloader.WebApi;

public class WorkshopTag
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}