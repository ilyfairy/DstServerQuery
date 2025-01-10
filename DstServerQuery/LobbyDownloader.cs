using DstServerQuery.Models;
using DstServerQuery.Models.Lobby;
using DstServerQuery.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DstServerQuery;

public record struct RegionPlatform(string Region, LobbyPlatform Platform);
public record struct RegionUrl(string BriefsUrl, string DetailsUrl);

public class LobbyDownloader
{
    public const string RegionCapabilitiesUrl = "https://lobby-v2-cdn.klei.com/regioncapabilities-v2.json";

    private readonly HttpClient _http;
    private readonly HttpClient _httpUpdate;
    private readonly DstWebConfig _dstWebConfig;
    private readonly ILogger? _logger;

    //通过区域和平台获取url
    public Dictionary<RegionPlatform, RegionUrl> RegionPlatformMap { get; set; } = [];

    public LobbyDownloader(DstWebConfig dstWebConfig, ILogger? logger = null)
    {
        this._dstWebConfig = dstWebConfig;
        this._logger = logger;
        static HttpClient Create()
        {
            HttpClientHandler handler = new();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.AutomaticDecompression = DecompressionMethods.All;
            HttpClient http = new(handler);

            var cacheHeader = new CacheControlHeaderValue();
            cacheHeader.NoCache = true;
            cacheHeader.NoStore = true;
            http.DefaultRequestHeaders.CacheControl = cacheHeader;

            http.Timeout = TimeSpan.FromSeconds(100);
            return http;
        }

        _http = Create();
        _httpUpdate = Create();
    }

    public async Task InitializeAsync()
    {
        //获取"区域&平台"的url映射
        string[] platforms = ["Steam", "PSN", "Rail", "XBone", "Switch"];
        async Task<string[]> GetRegions()
        {
            var stream = await _http.GetStreamAsync(RegionCapabilitiesUrl);
            var obj = JsonNode.Parse(stream);
            return obj?["LobbyRegions"]?.AsArray().Select(v => v!["Region"]!.GetValue<string>()).ToArray() ?? [];
        }
        foreach (var region in await GetRegions())
        {
            foreach (var platform in platforms)
            {
                RegionPlatformMap[new(region, Enum.Parse<LobbyPlatform>(platform))] =
                    new(
                        _dstWebConfig.LobbyProxyTemplate.Replace("{region}", region).Replace("{platform}", platform),
                        $"https://lobby-v2-{region}.klei.com/lobby/read"
                    );
            }
        }
    }

    //public IEnumerable<string> RegionUrls => RegionPlatformMap.Values.Select(v => v.BriefsUrl);

    //GET请求
    private async Task<IReadOnlyList<LobbyServer>> DownloadBriefs(string url, CancellationToken cancellationToken = default)
    {
        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var get = await response.Content.ReadFromJsonAsync<LobbyGet<LobbyServerDetailed>>(cancellationToken).ConfigureAwait(false);

        //var get = await response.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);

        if (get?.Data is null) return [];

        foreach (var item in get.Data)
        {
            item._LastUpdate = DateTimeOffset.Now;
        }
        return get.Data;
    }

    //private async Task<JsonArray> DownloadBriefsJson(string url, CancellationToken cancellationToken = default)
    //{
    //    var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    //    var json = await response.Content.ReadAsStringAsync(cancellationToken);

    //    JsonObject? obj = JsonNode.Parse(json)?.AsObject();

    //    return obj?["GET"]?.AsArray() ?? [];
    //}



    ////POST请求
    //public async Task<LobbyServerDetailed[]> DownloadDetails(string url, CancellationToken cancellationToken = default)
    //{
    //    var body =
    //        $$$"""
    //        {
    //            "__gameId": "DontStarveTogether",
    //            "__token": "{{{dstWebConfig.Token}}}",
    //            "query": {}
    //        }
    //        """;
    //    HttpRequestMessage reqeustMessage = new(HttpMethod.Post, url)
    //    {
    //        Content = new StringContent(body, null, MediaTypeNames.Application.Json)
    //    };
    //    var r = await http.SendAsync(reqeustMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    //    GET<LobbyServerDetailed>? get;
    //    try
    //    {
    //        get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);
    //    }
    //    catch (Exception e)
    //    {
    //        return [];
    //    }
    //    if (get is null || get.Data is null) return [];
    //    foreach (var item in get.Data)
    //    {
    //        item._IsDetails = true;
    //        item._LastUpdate = DateTimeOffset.Now;
    //    }
    //    return get.Data;
    //}


    //POST请求
    public async ValueTask<bool> UpdateToDetails(LobbyServerDetailed server, CancellationToken cancellationToken = default)
    {
        var proxyUrl = GetProxyUrl();
        async ValueTask<bool> Request(string? url)
        {
            HttpContent requestBody;
            if (string.IsNullOrWhiteSpace(url))
            {
                // https://lobby-v2-cdn.klei.com/{Region}-{Platform}.json.gz
                if (server._Region == null) return false;
                if (!RegionPlatformMap.TryGetValue(new(server._Region, server._LobbyPlatform), out var regionUrl)) return false;
                url = regionUrl.DetailsUrl;
                //var request = new RestRequest(url.details, Method.Post);
                string str =
                $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{_dstWebConfig.Token}}}",
                    "query": {
                        "__rowId": "{{{server.RowId}}}"
                    }
                }
                """;
                requestBody = new StringContent(str, Encoding.UTF8, MediaTypeNames.Application.Json);
            }
            else
            {
                object[] requestList = [new
                {
                    server.RowId,
                    Region = server._Region,
                }];
                requestBody = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
            }

            var r = await _httpUpdate.PostAsync(url, requestBody, cancellationToken);
            var get = await r.Content.ReadFromJsonAsync<LobbyGet<LobbyServerDetailed>>(cancellationToken);
            if (get is null || get.Data is null || get.Data.Count == 0) return false;

            var newServer = get.Data.FirstOrDefault();
            if (newServer is null)
            {
                return false;
            }
            newServer._LastUpdate = DateTimeOffset.Now;
            newServer._LobbyPlatform = server._LobbyPlatform;
            newServer._Region = server._Region;
            newServer._IsDetailed = true;

            //newServer.CopyTo(server); //更新数据
            //server._IsDetails = true; //变成详细数据
            //server._LastUpdate = DateTimeOffset.Now;
            server.UpdateFrom(newServer);

            return true;
        }
        try
        {
            if (await Request(proxyUrl))
            {
                return true;
            }
            else
            {
                return await Request(null);
            }
        }
        catch (Exception)
        {
            return await Request(null);
        }
    }

    private readonly List<LobbyServerDetailed> _updatedChunksCache = new(1100);
    //POST请求
    public async Task<int> UpdateToDetails(IDictionary<string, LobbyServerDetailed> servers, Action<ICollection<LobbyServerDetailed>>? chunkCallback, CancellationToken cancellationToken = default)
    {
        if (servers.Count == 0) return 0;

        int updatedCount = 0;
        //int requestCount = 0;
        //List<LobbyServerDetailed> updatedClone = new(servers.Count);

        var proxyUrl = GetProxyUrl();
        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            foreach (var item in servers)
            {
                //requestCount++;
                if (await UpdateToDetails(item.Value, cancellationToken))
                {
                    //updatedClone.Add(item);
                    Interlocked.Increment(ref updatedCount);
                }
            }
        }
        else
        {
            // RegionPlatform, LobbyServerDetailed
            var requests = servers.Values.Select(v =>
            {
                if (v._Region is null) return null;

                var region = new RegionPlatform(v._Region, v._LobbyPlatform);
                if (RegionPlatformMap.ContainsKey(region))
                {
                    //return new
                    //{
                    //    Region = region,
                    //    Server = v,
                    //};
                    return new (RegionPlatform Region, LobbyServerDetailed Server)?((region, v));
                }
                return null;
            }).Where(v => v is not null);

            //var rowIdMap = servers.ToDictionary(v => v.RowId);

            //并行更新
            ParallelOptions opt = new()
            {
                MaxDegreeOfParallelism = _dstWebConfig.UpdateThreads ?? 6,
                CancellationToken = cancellationToken
            };
            await Parallel.ForEachAsync(requests.Chunk(50), opt, async (chunk, ct) =>
            {
                var requestList = chunk.Select(v =>
                    new RowIdRegion(v!.Value.Server.RowId, v!.Value.Region.Region));

                HttpResponseMessage r;
                var content = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
                try
                {
                    r = await _httpUpdate.PostAsync(proxyUrl, content, ct);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("SSL"))
                    {
                        //Debug.Assert(false);
                    }
                    return;
                }

                LobbyGet<LobbyServerDetailed>? get;
                try
                {
                    if (string.Equals(r.Content.Headers.ContentType?.MediaType, MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
                    {
                        get = await r.Content.ReadFromJsonAsync<LobbyGet<LobbyServerDetailed>>(ct);
                    }
                    else
                    {
                        _logger?.LogError("服务器详细信息下载失败 响应不是Json Url:{Url} 响应:{Body}", proxyUrl, await r.Content.ReadAsStringAsync(default));
                        return;
                    }
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "服务器详细信息下载失败 Url:{Url}", proxyUrl);
                    return;
                }
                var dateTime = DateTimeOffset.Now;

                if (get is null || get.Data is null) return;

                Array50 serverTemp = default;
                int index = 0;
                foreach (var newServer in get.Data)
                {
                    if (servers.TryGetValue(newServer.RowId, out var server))
                    {
                        newServer._LastUpdate = dateTime;
                        newServer._Region = server._Region;
                        newServer._LobbyPlatform = server._LobbyPlatform;
                        newServer._IsDetailed = true;
                        //server.Raw = newRaw;
                        //server.Update();
                        server.UpdateFrom(newServer);

                        //lock (updatedChunks)
                        //{
                        //    //updatedClone.Add(server.Clone());
                        //}
                        serverTemp[index] = server;
                        index++;
                    }
                }
                Interlocked.Add(ref updatedCount, get.Data.Count);
                lock (this)
                {
                    for (int i = 0; i < index; i++)
                    {
                        _updatedChunksCache.Add(serverTemp[i]);
                    }
                    if (_updatedChunksCache.Count > 1000)
                    {
                        chunkCallback?.Invoke(_updatedChunksCache.ToArray());
                        _updatedChunksCache.Clear();
                    }
                }
            });
            if (_updatedChunksCache.Count != 0)
            {
                chunkCallback?.Invoke(_updatedChunksCache);
                _updatedChunksCache.Clear();
            }
        }

        return updatedCount;
    }

    public async IAsyncEnumerable<LobbyServer> DownloadAllBriefs([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var map in RegionPlatformMap)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var r = await DownloadBriefs(map.Value.BriefsUrl, cancellationToken).ConfigureAwait(false);
            foreach (var item in r)
            {
                item._LastUpdate = DateTimeOffset.Now;
                item._Region = map.Key.Region;
                item._LobbyPlatform = map.Key.Platform;
                yield return item;
            }
        }
    }

    //public async Task<JsonArray> DownloadAllBriefsJson(CancellationToken cancellationToken = default)
    //{
    //    JsonArray arr = new();
    //    foreach (var map in RegionPlatformMap)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();
    //        var r = await DownloadBriefsJson(map.Value.BriefsUrl, cancellationToken);
    //        foreach (var item in r)
    //        {
    //            arr.Add(item.DeepClone());
    //        }
    //    }
    //    return arr;
    //}

    public string? GetProxyUrl()
    {
        var dstDetailsProxyUrls = _dstWebConfig.DstDetailsProxyUrls;

        if (dstDetailsProxyUrls is null or { Length: 0 }) return null;
        var rand = Random.Shared.Next() % dstDetailsProxyUrls.Length;
        return dstDetailsProxyUrls[rand];
    }

}


[InlineArray(50)]
file struct Array50
{
    public LobbyServerDetailed First { get; set; }
}

file record struct RowIdRegion(string RowId, string Region);