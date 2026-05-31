using System.Text.Json;
using SteamDownloader.Helpers;

namespace SteamDownloader.WebApi.Interfaces;

/// <summary>
/// see https://partner.steamgames.com/doc/webapi/ISteamRemoteStorage
/// </summary>
/// <param name="steamSession"></param>
public class SteamRemoteStorage(SteamSession steamSession) : InterfaceBase(steamSession)
{
    public async Task<GetPublishedFileDetailsResponse> GetPublishedFileDetails(ulong[] publishedfileids, CancellationToken cancellationToken = default)
    {
        List<KeyValuePair<string, string?>> list = new(publishedfileids.Length + 2);

        list.Add(new("itemcount", publishedfileids.Length.ToString()));

        for (int i = 0; i < publishedfileids.Length; i++)
        {
            list.Add(new($"publishedfileids[{i}]", publishedfileids[i].ToString()));
        }

        Uri url = new(WebApiBaseAddress, "/ISteamRemoteStorage/GetPublishedFileDetails/v1/");
        FormUrlEncodedContent content = new(list);
        var response = await steamSession.HttpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        WebApiResponse<GetPublishedFileDetailsResponse>? r = await JsonSerializer.DeserializeAsync<WebApiResponse<GetPublishedFileDetailsResponse>>(stream, JsonOptions, cancellationToken);

        return r?.Response ?? throw new ArgumentNullException("response");
    }
}
