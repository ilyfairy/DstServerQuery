using System.Text.Json.Serialization;

namespace SteamDownloader.WebApi;

public class WebApiResponse<T>
{
    [JsonPropertyName("response")]
    public T? Response { get; set; }
}