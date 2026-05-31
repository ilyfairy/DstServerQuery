using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SteamDownloader.WebApi;

public class QueryFilesResponse
{
    [JsonPropertyName("total")]
    public uint Total { get; set; }

    [JsonPropertyName("publishedfiledetails")]
    public WorkshopFileDetails[]? PublishedFileDetails { get; set; }
}
