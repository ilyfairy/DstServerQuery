using SteamKit2.CDN;
using System.Net;
using System.Text.Json.Serialization;

/// <summary>
/// <see cref="SteamKit2.CDN.Server"/>
/// </summary>
public record SteamContentServer
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("source_id")]
    public int SourceId { get; set; }

    [JsonPropertyName("cell_id")]
    public uint CellId { get; set; }

    [JsonPropertyName("load")]
    public int Load { get; set; }

    [JsonPropertyName("weighted_load")]
    public int WeightedLoad { get; set; }

    [JsonPropertyName("num_entries_in_client_list")]
    public int NumEntriesInClientList { get; set; }

    [JsonPropertyName("host")]
    public required string Host { get; init; }

    [JsonPropertyName("vhost")]
    public required string Vhost { get; init; }

    [JsonPropertyName("https_support")]
    public string? HttpsSupport { get; set; }

    [JsonPropertyName("preferred_server")]
    public bool PreferredServer { get; set; }

    [JsonPropertyName("priority_class")]
    public int PriorityClass { get; set; }

    private Uri? url;
    public Uri Url
    {
        get
        {
            url ??= new UriBuilder("https", Host).Uri;
            return url;
        }
    }

    public static SteamContentServer FromHost(string host)
    {
        SteamContentServer contentServer = new()
        {
            Host = host,
            Vhost = host,
        };
        return contentServer;
    }

    public static implicit operator Server(SteamContentServer contentServer)
    {
        return (Server)new DnsEndPoint(contentServer.Host, 443);
    }
}