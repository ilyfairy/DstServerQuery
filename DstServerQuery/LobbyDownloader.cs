﻿using DstServerQuery.Models;
using DstServerQuery.Models.Lobby;
using DstServerQuery.Models.Requests;
using Serilog;
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

    private readonly HttpClient http;
    private readonly HttpClient httpUpdate;
    private readonly DstWebConfig dstWebConfig;

    //通过区域和平台获取url
    public Dictionary<RegionPlatform, RegionUrl> RegionPlatformMap { get; set; } = new();

    public LobbyDownloader(DstWebConfig dstWebConfig)
    {
        this.dstWebConfig = dstWebConfig;

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

        http = Create();
        httpUpdate = Create();
    }

    public async Task InitializeAsync()
    {
        //获取"区域&平台"的url映射
        var platforms = new[] { "Steam", "PSN", "Rail", "XBone", "Switch" };
        async Task<string[]> GetRegions()
        {
            var stream = await http.GetStreamAsync(RegionCapabilitiesUrl);
            var obj = JsonNode.Parse(stream);
            return obj?["LobbyRegions"]?.AsArray().Select(v => v!["Region"]!.GetValue<string>()).ToArray() ?? Array.Empty<string>();
        }
        foreach (var region in await GetRegions())
        {
            foreach (var platform in platforms)
            {
                RegionPlatformMap[new(region, Enum.Parse<LobbyPlatform>(platform))] =
                    new(
                        dstWebConfig.LobbyProxyTemplate.Replace("{region}", region).Replace("{platform}", platform),
                        $"https://lobby-v2-{region}.klei.com/lobby/read"
                    );
            }
        }
    }

    //public IEnumerable<string> RegionUrls => RegionPlatformMap.Values.Select(v => v.BriefsUrl);

    //GET请求
    private async Task<List<LobbyServerRaw>> DownloadBriefs(string url, CancellationToken cancellationToken = default)
    {
        var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var get = await response.Content.ReadFromJsonAsync<GET<LobbyServerRaw>>(cancellationToken).ConfigureAwait(false);

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
                if (server.Raw._Region == null) return false;
                if (!RegionPlatformMap.TryGetValue(new(server.Raw._Region, server.Raw._LobbyPlatform), out var regionUrl)) return false;
                url = regionUrl.DetailsUrl;
                //var request = new RestRequest(url.details, Method.Post);
                string str =
                $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{dstWebConfig.Token}}}",
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
                    Region = server.Raw._Region,
                }];
                requestBody = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
            }

            var r = await httpUpdate.PostAsync(url, requestBody, cancellationToken);
            var get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailedRaw>>(cancellationToken);
            if (get is null || get.Data is null || get.Data.Count == 0) return false;

            var newRaw = get.Data.FirstOrDefault();
            if (newRaw is null)
            {
                return false;
            }
            newRaw._LastUpdate = DateTimeOffset.Now;
            newRaw._LobbyPlatform = server.Raw._LobbyPlatform;
            newRaw._Region = server.Raw._Region;
            newRaw._IsDetailed = true;

            //newServer.CopyTo(server); //更新数据
            //server._IsDetails = true; //变成详细数据
            //server._LastUpdate = DateTimeOffset.Now;
            server.Raw = newRaw;

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

    private readonly List<LobbyServerDetailed> updatedChunksCache = new(1100);
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
                if (v.Raw is null || v.Raw._Region is null) return null;

                var region = new RegionPlatform(v.Raw._Region, v.Raw._LobbyPlatform);
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
                MaxDegreeOfParallelism = dstWebConfig.UpdateThreads ?? 6,
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
                    r = await httpUpdate.PostAsync(proxyUrl, content, ct);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("SSL"))
                    {
                        //Debug.Assert(false);
                    }
                    return;
                }

                GET<LobbyServerDetailedRaw>? get;
                try
                {
                    get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailedRaw>>(ct);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error("{Exception}", e);
                    //var requestJson = JsonSerializer.Serialize(requestList);
                    //throw;
                    return;
                }
                var dateTime = DateTimeOffset.Now;

                if (get is null || get.Data is null) return;

                Array50 serverTemp = default;
                int index = 0;
                foreach (var newRaw in get.Data)
                {
                    if (servers.TryGetValue(newRaw.RowId, out var server))
                    {
                        newRaw._LastUpdate = dateTime;
                        newRaw._Region = server.Raw!._Region;
                        newRaw._LobbyPlatform = server.Raw._LobbyPlatform;
                        newRaw._IsDetailed = true;
                        //server.Raw = newRaw;
                        //server.Update();
                        server.UpdateFrom(newRaw);

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
                        updatedChunksCache.Add(serverTemp[i]);
                    }
                    if (updatedChunksCache.Count > 1000)
                    {
                        chunkCallback?.Invoke(updatedChunksCache.ToArray());
                        updatedChunksCache.Clear();
                    }
                }
            });
            if (updatedChunksCache.Count != 0)
            {
                chunkCallback?.Invoke(updatedChunksCache);
                updatedChunksCache.Clear();
            }
        }

        return updatedCount;
    }

    public async IAsyncEnumerable<LobbyServerRaw> DownloadAllBriefs([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        var dstDetailsProxyUrls = dstWebConfig.DstDetailsProxyUrls;

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

[InlineArray(100000)]
file struct Array100000
{
    public object First { get; set; }
}

file record struct RowIdRegion(string RowId, string Region);