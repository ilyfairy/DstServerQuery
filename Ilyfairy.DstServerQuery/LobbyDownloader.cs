using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
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
        private readonly string _token;
        private readonly string[]? _dstDetailsProxyUrls; // https://api.com/{0}/{1}/{2}/{3}

        //通过区域和平台获取url
        public Dictionary<RegionPlatform, RegionUrl> RegionPlatformMap { get; set; } = new();

        public LobbyDownloader(DstJsonOptions dstJsonOptions, string dstToken, string[]? dstDetailsProxyUrls = null)
        {
            this.dstJsonOptions = dstJsonOptions;
            _token = dstToken;

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
            _dstDetailsProxyUrls = dstDetailsProxyUrls;
        }

        public async Task Initialize()
        {
            //获取"区域&平台"的url映射
            var platforms = new[] { "Steam", "PSN", "Rail", "XBone", "Switch" };
            async Task<string[]> GetRegions()
            {
                var stream = await http.GetStreamAsync(RegionCapabilitiesUrl);
                var obj = JsonNode.Parse(stream);
                return obj?["LobbyRegions"]?.AsArray().Select(v => v["Region"].GetValue<string>()).ToArray() ?? Array.Empty<string>();
            }
            foreach (var region in await GetRegions())
            {
                foreach (var platform in platforms)
                {
                    RegionPlatformMap[new(region, Enum.Parse<LobbyPlatform>(platform))] =
                        new(
                            $"https://lobby-v2-cdn.klei.com/{region}-{platform}.json.gz",
                            $"https://lobby-v2-{region}.klei.com/lobby/read"
                        );
                }
            }
        }

        //GET请求
        private async Task<LobbyServerDetailed[]> DownloadBriefs(string url, CancellationToken cancellationToken = default)
        {
            var response = await http.SendAsync(new(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead, cancellationToken);
            var get = await response.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);

            if (get?.Data is null) return Array.Empty<LobbyServerDetailed>();

            foreach (var item in get.Data)
            {
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        //POST请求
        public async Task<LobbyServerDetailed[]> DownloadDetails(string url, CancellationToken cancellationToken = default)
        {
            var body = $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{_token}}}",
                    "query": {}
                }
                """;
            HttpRequestMessage msg = new(HttpMethod.Post, url);
            msg.Content = new StringContent(body, null, MediaTypeNames.Application.Json);
            var r = await http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            GET<LobbyServerDetailed>? get;
            try
            {
                get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);
            }
            catch (Exception e)
            {
                return Array.Empty<LobbyServerDetailed>();
            }
            if (get is null || get.Data is null) return Array.Empty<LobbyServerDetailed>();
            foreach (var item in get.Data)
            {
                item._IsDetails = true;
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        //POST请求
        public async ValueTask<int> UpdateToDetails(LobbyServerDetailed data, CancellationToken cancellationToken = default)
        {
            var url = GetProxyUrl();
            HttpContent body;
            if (string.IsNullOrWhiteSpace(url))
            {
                // https://lobby-v2-cdn.klei.com/{Region}-{Platform}.json.gz
                if (data._Region == null) return 0;
                if (!RegionPlatformMap.TryGetValue(new(data._Region, data._LobbyPlatform), out var regionUrl)) return 0;
                url = regionUrl.DetailsUrl;
                //var request = new RestRequest(url.details, Method.Post);
                string str = $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{_token}}}",
                    "query": {
                        "__rowId": "{{{data.RowId}}}"
                    }
                }
                """;
                body = new StringContent(str, Encoding.UTF8, MediaTypeNames.Application.Json);
            }
            else
            {
                object[] requestList = [new
                {
                    RowId = data.RowId,
                    Region = data._Region
                }];
                body = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
            }

            var r = await httpUpdate.PostAsync(url, body, cancellationToken);
            var get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, cancellationToken);
            if (get is null || get.Data is null || !get.Data.Any()) return 0;

            var newData = get.Data.First();
            newData.CopyTo(data); //更新数据
            data._IsDetails = true; //变成详细数据
            data._LastUpdate = DateTime.Now;
            return 1;
        }

        public async ValueTask<int> UpdateToDetails(ICollection<LobbyServerDetailed> servers, CancellationToken cancellationToken = default)
        {
            if (servers.Count == 0) return 0;

            int updatedCount = 0;
            int requestCount = 0;

            var proxyUrl = GetProxyUrl();
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                foreach (var item in servers)
                {
                    requestCount++;
                    if (await UpdateToDetails(item, cancellationToken) > 0)
                    {
                        updatedCount++;
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
                    MaxDegreeOfParallelism = 16,
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
                    try
                    {
                        var content = JsonContent.Create(requestList, null, JsonSerializerOptions.Default);
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
                    var get = await r.Content.ReadFromJsonAsync<GET<LobbyServerDetailed>>(dstJsonOptions.DeserializerOptions, ct);
                    requestCount++;
                    if (get is null || get.Data is null) return;
                    foreach (var newData in get.Data)
                    {
                        if (rowIdMap.TryGetValue(newData.RowId, out var data))
                        {
                            updatedCount++;
                            newData.CopyTo(data); //更新数据
                            data._IsDetails = true; //标记为详细数据
                            data._LastUpdate = DateTime.Now;
                        }
                    }
                });
            }
            return updatedCount;
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

        public string? GetProxyUrl()
        {
            if (_dstDetailsProxyUrls is null or { Length: 0 }) return null;
            var rand = Random.Shared.Next() % _dstDetailsProxyUrls.Length;
            return _dstDetailsProxyUrls[rand];
        }

    }
}
