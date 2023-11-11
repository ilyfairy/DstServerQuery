using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Models.Requests;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ilyfairy.DstServerQuery
{
    public record RegionPlatform(string Region, LobbyPlatform Platform);
    public record RegionUrl(string BriefsUrl, string DetailsUrl);

    public class LobbyDownloader
    {
        public const string RegionCapabilitiesUrl = "https://lobby-v2-cdn.klei.com/regioncapabilities-v2.json";

        private readonly HttpClient http;
        private readonly HttpClient httpUpdate;
        private readonly DstJsonOptions dstJsonOptions;
        private readonly DstWebConfig dstWebConfig;

        //通过区域和平台获取url
        public Dictionary<RegionPlatform, RegionUrl> RegionPlatformMap { get; set; } = new();

        public LobbyDownloader(DstJsonOptions dstJsonOptions, DstWebConfig dstWebConfig)
        {
            this.dstJsonOptions = dstJsonOptions;
            this.dstWebConfig = dstWebConfig;

            static HttpClient Create()
            {
                HttpClientHandler handler = new();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Brotli;
                HttpClient http = new(handler);
                http.Timeout = TimeSpan.FromSeconds(100);
                return http;
            }

            this.http = Create();
            this.httpUpdate = Create();
        }

        public async Task Initialize()
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

        //GET请求
        private async Task<LobbyServerDetailed[]> DownloadBriefs(string url, CancellationToken cancellationToken = default)
        {
            var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var get = await response.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);

            if (get?.Data is null) return [];
            
            foreach (var item in get.Data)
            {
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        private async Task<JsonArray> DownloadBriefsJson(string url, CancellationToken cancellationToken = default)
        {
            var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            JsonObject? obj = JsonNode.Parse(json)?.AsObject();

            return obj?["GET"]?.AsArray() ?? [];
        }

        

        //POST请求
        public async Task<LobbyServerDetailed[]> DownloadDetails(string url, CancellationToken cancellationToken = default)
        {
            var body = $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{dstWebConfig.Token}}}",
                    "query": {}
                }
                """;
            HttpRequestMessage reqeustMessage = new(HttpMethod.Post, url)
            {
                Content = new StringContent(body, null, MediaTypeNames.Application.Json)
            };
            var r = await http.SendAsync(reqeustMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            GET<LobbyServerDetailed>? get;
            try
            {
                get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);
            }
            catch (Exception e)
            {
                return [];
            }
            if (get is null || get.Data is null) return [];
            foreach (var item in get.Data)
            {
                item._IsDetails = true;
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        //POST请求
        public async ValueTask<bool> UpdateToDetails(LobbyServerDetailed server, CancellationToken cancellationToken = default)
        {
            var proxyUrl = GetProxyUrl();
            async ValueTask<bool> Request(string? url)
            {
                HttpContent body;
                if (string.IsNullOrWhiteSpace(url))
                {
                    // https://lobby-v2-cdn.klei.com/{Region}-{Platform}.json.gz
                    if (server._Region == null) return false;
                    if (!RegionPlatformMap.TryGetValue(new(server._Region, server._LobbyPlatform), out var regionUrl)) return false;
                    url = regionUrl.DetailsUrl;
                    //var request = new RestRequest(url.details, Method.Post);
                    string str = $$$"""
                    {
                        "__gameId": "DontStarveTogether",
                        "__token": "{{{dstWebConfig.Token}}}",
                        "query": {
                            "__rowId": "{{{server.RowId}}}"
                        }
                    }
                    """;
                    body = new StringContent(str, Encoding.UTF8, MediaTypeNames.Application.Json);
                }
                else
                {
                    object[] requestList = [new
                    {
                        RowId = server.RowId,
                        Region = server._Region
                    }];
                    body = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
                }

                var r = await httpUpdate.PostAsync(url, body, cancellationToken);
                var get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);
                if (get is null || get.Data is null || get.Data.Length == 0) return false;

                var newServer = get.Data.First();
                newServer.CopyTo(server); //更新数据
                server._IsDetails = true; //变成详细数据
                server._LastUpdate = DateTime.Now;
                return true;
            }
            try
            {
                if(await Request(proxyUrl))
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

        //POST请求
        public async Task<ICollection<LobbyServerDetailed>> UpdateToDetails(ICollection<LobbyServerDetailed> servers, CancellationToken cancellationToken = default)
        {
            if (servers.Count == 0) return [];

            //int updatedCount = 0;
            //int requestCount = 0;
            ConcurrentBag<LobbyServerDetailed> updated = new();

            var proxyUrl = GetProxyUrl();
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                foreach (var item in servers)
                {
                    //requestCount++;
                    if (await UpdateToDetails(item, cancellationToken))
                    {
                        updated.Add(item);
                        //Interlocked.Increment(ref  updatedCount);
                    }
                }
            }
            else
            {
                List<KeyValuePair<RegionPlatform, LobbyServerDetailed>> requests = new(servers.Count);

                foreach (var item in servers)
                {
                    if (item._Region is null) continue;

                    var region = new RegionPlatform(item._Region, item._LobbyPlatform);
                    if (RegionPlatformMap.ContainsKey(region))
                    {
                        requests.Add(new KeyValuePair<RegionPlatform, LobbyServerDetailed>(region, item));
                    }
                }

                var rowIdMap = servers.ToDictionary(v => v.RowId);

                ParallelOptions opt = new()
                {
                    MaxDegreeOfParallelism = 5,
                    CancellationToken = cancellationToken
                };
                await Parallel.ForEachAsync(requests.Chunk(50), opt, async (chunk, ct) =>
                {
                    List<object> requestList = new();
                    foreach (var item in chunk)
                    {
                        //输入 [{RowId:xxx,Region:xxx}]
                        requestList.Add(new
                        {
                            RowId = item.Value.RowId,
                            Region = item.Key.Region
                        });
                    }

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

                    GET<LobbyServerDetailed>? get;
                    try
                    {
                         get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, ct);
                    }
                    catch (Exception e)
                    {
                        var requestJson = JsonSerializer.Serialize(requestList);
                        //throw;
                        return;
                    }

                    //Interlocked.Increment(ref requestCount);
                    if (get is null || get.Data is null) return;
                    foreach (var newServer in get.Data)
                    {
                        if (rowIdMap.TryGetValue(newServer.RowId, out var server))
                        {
                            updated.Add(server);
                            //Interlocked.Increment(ref updatedCount);
                            newServer.CopyTo(server); //更新数据
                            server._IsDetails = true; //标记为详细数据
                            server._LastUpdate = DateTime.Now;
                        }
                    }
                });
            }

            return updated.ToArray();
        }

        public async IAsyncEnumerable<LobbyServerDetailed> DownloadAllBriefs([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var map in RegionPlatformMap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var r = await DownloadBriefs(map.Value.BriefsUrl, cancellationToken);
                foreach (var item in r)
                {
                    item._Region = map.Key.Region;
                    item._LobbyPlatform = map.Key.Platform;
                    item._LastUpdate = DateTime.Now;
                    yield return item;
                }
            }
        }

        public async Task<JsonArray> DownloadAllBriefsJson(CancellationToken cancellationToken = default)
        {
            JsonArray arr = new();
            foreach (var map in RegionPlatformMap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var r = await DownloadBriefsJson(map.Value.BriefsUrl, cancellationToken);
                foreach (var item in r)
                {
                    arr.Add(item.DeepClone());
                }
            }
            return arr;
        }

        public string? GetProxyUrl()
        {
            var dstDetailsProxyUrls = dstWebConfig.DstDetailsProxyUrls;

            if (dstDetailsProxyUrls is null or { Length: 0 }) return null;
            var rand = Random.Shared.Next() % dstDetailsProxyUrls.Length;
            return dstDetailsProxyUrls[rand];
        }

    }
}
