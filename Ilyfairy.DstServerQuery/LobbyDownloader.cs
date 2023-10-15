using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
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

        //通过区域和平台获取url (适用于新版数据)
        public Dictionary<RegionPlatform, RegionUrl> BriefsUrlMap { get; set; } = new();

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
                    BriefsUrlMap[new(region, Enum.Parse<LobbyPlatform>(platform))] =
                        new (
                            $"https://lobby-v2-cdn.klei.com/{region}-{platform}.json.gz",
                            $"https://lobby-v2-{region}.klei.com/lobby/read"
                        );
                }
            }
        }

        //GET请求
        private async Task<List<LobbyDetailsData>> DownloadBriefs(string url, CancellationToken cancellationToken = default)
        {
            var response = await http.SendAsync(new(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead, cancellationToken);
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            MemoryStream ms = new();
            await stream.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var get = JsonSerializer.Deserialize<GET<LobbyDetailsData>>(ms, dstJsonOptions.DeserializerOptions);
            if (get?.Data is null) return new();

            foreach (var item in get.Data)
            {
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        //POST请求
        public async Task<List<LobbyDetailsData>> DownloadDetails(string url, CancellationToken cancellationToken = default)
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
            var r = await http.SendAsync(msg, cancellationToken);
            var get = await r.Content.ReadFromJsonAsync<GET<LobbyDetailsData>>();
            if (get is null || get.Data is null) return new();
            foreach (var item in get.Data)
            {
                item._IsDetails = true;
                item._LastUpdate = DateTime.Now;
            }
            return get.Data;
        }

        //POST请求
        public async ValueTask<int> UpdateToDetails(LobbyDetailsData data, CancellationToken cancellationToken = default)
        {
            // https://lobby-v2-cdn.klei.com/{Region}-{Platform}.json.gz
            if (!BriefsUrlMap.TryGetValue(new(data._Region, data._LobbyPlatform), out var url)) return 0;
            //var request = new RestRequest(url.details, Method.Post);
            string body = $$$"""
                {
                    "__gameId": "DontStarveTogether",
                    "__token": "{{{_token}}}",
                    "query": {
                        "__rowId": "{{{data.RowId}}}"
                    }
                }
                """;

            var r = await httpUpdate.PostAsync(url.DetailsUrl, new StringContent(body, null, MediaTypeNames.Application.Json), cancellationToken);
            var get = await r.Content.ReadFromJsonAsync<GET<LobbyDetailsData>>(default(JsonSerializerOptions), cancellationToken);
            if (get is null || get.Data is null || !get.Data.Any()) return 0;
            var newData = get.Data.First();
            newData.CopyTo(data); //更新数据
            data._IsDetails = true; //变成详细数据
            data._LastUpdate = DateTime.Now;
            return 1;
        }

        public async ValueTask<int> UpdateToDetails(LobbyDetailsData[] datas, CancellationToken cancellationToken = default)
        {
            if(datas.Length == 0) return 0;
            
            int updatedCount = 0;
            int requestCount = 0;

            var proxyUrl = GetProxyUrl();
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                foreach (var item in datas)
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
                var group = datas.GroupBy(v =>
                {
                    var region = new RegionPlatform(v._Region, v._LobbyPlatform);
                    if (BriefsUrlMap.TryGetValue(region, out var url))
                    {
                        return region;
                    }
                    else
                    {
                        return null;
                    }
                }).Where(v => v.Key != null);

                var regionMap = group.ToDictionary(v => v.Key!, v => v.Cast<LobbyDetailsData>());

                List<KeyValuePair<RegionPlatform, LobbyDetailsData>> requests = new(regionMap.Sum(v => v.Value.Count()));
                foreach (var region in regionMap)
                {
                    foreach (var data in region.Value)
                    {
                        requests.Add(new(region.Key, data));
                    }
                }
                var rowIdMap = datas.ToDictionary(v => v.RowId);

                ParallelOptions opt = new()
                {
                    MaxDegreeOfParallelism = 16,
                    CancellationToken = cancellationToken
                };
                await Parallel.ForEachAsync(requests.Chunk(50), opt, async (chunk, ct) =>
                {
                    await Task.Yield();
                    List<object> requestList = new();
                    foreach (var item in chunk)
                    {
                        requestList.Add(new
                        {
                            RowId = item.Value.RowId,
                            Region = item.Key.Region
                        });
                    }
                    var body = JsonSerializer.Serialize(requestList);
                    var url = _dstDetailsProxyUrls;
                    HttpResponseMessage r;
                    try
                    {
                        r = await httpUpdate.PostAsync(proxyUrl, new StringContent(body, null, MediaTypeNames.Application.Json), ct);
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Contains("SSL"))
                        {
                            Debug.Assert(false);
                        }
                        return;
                    }
                    var json = await r.Content.ReadAsStringAsync(ct);
                    var get = JsonSerializer.Deserialize<GET<LobbyDetailsData>>(json, default(JsonSerializerOptions));
                    requestCount++;
                    if (get is null || get.Data is null || !get.Data.Any()) return;
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

        public async Task<List<LobbyDetailsData>> DownloadAllBriefs(CancellationToken cancellationToken = default)
        {
            ConcurrentBag<List<LobbyDetailsData>> list = new();
            foreach (var map in BriefsUrlMap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var r = await DownloadBriefs(map.Value.BriefsUrl, cancellationToken);
                foreach (var item in r)
                {
                    item._Region = map.Key.Region;
                    item._LobbyPlatform = map.Key.Platform;
                    item._LastUpdate = DateTime.Now;
                }
                list.Add(r);
            }
            List<LobbyDetailsData> datas = new(list.Sum(v => v.Count));
            foreach (var region in list)
            {
                foreach (var item in region)
                {
                    datas.Add(item);
                }
            }
            return datas;
        }

        public string? GetProxyUrl()
        {
            if (_dstDetailsProxyUrls == null) return null;
            var rand = Random.Shared.Next() % _dstDetailsProxyUrls.Length;
            return _dstDetailsProxyUrls[rand];
        }

    }
}
