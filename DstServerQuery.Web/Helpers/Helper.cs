using System.Buffers;
using DstDownloaders;
using DstServerQuery.Web.Models.Configurations;
using SteamDownloader;
using SteamDownloader.Helpers;
using SteamKit2;

namespace DstServerQuery.Web.Helpers;

public static class Helper
{
    public static string GetRandomColor(int minGray, int maxGray)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minGray);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxGray, 255);

        var temp = ArrayPool<byte>.Shared.Rent(3);
        int gray;

        do
        {
            Random.Shared.NextBytes(temp.AsSpan()[0..3]);
            gray = (int)(0.299f * temp[0] + 0.587f * temp[1] + 0.114f * temp[2]);
        } while (gray < minGray || gray > maxGray);

        var colorHex = $"{temp[0]:X2}{temp[1]:X2}{temp[2]:X2}";
        ArrayPool<byte>.Shared.Return(temp);

        return colorHex;
    }

    public static SteamSession CreateSteamSession(IServiceProvider serviceProvider)
    {
        var steamOptions = serviceProvider.GetRequiredService<SteamOptions>();
        return new SteamSession(SteamConfiguration.Create(steamBuilder =>
        {
            if (steamOptions.SteampoweredApiProxy is { })
            {
                steamBuilder.WithWebAPIBaseAddress(steamOptions.SteampoweredApiProxy);
            }
            if (steamOptions.WebApiKey is { })
            {
                steamBuilder.WithWebAPIKey(steamOptions.WebApiKey);
            }
        }));
    }

    public static async Task EnsureContentServerAsync(DstDownloader dst, CancellationToken cancellationToken = default)
    {
        var servers1 = await dst.Steam.GetCdnServersAsync(1, null, cancellationToken);
        var servers2 = await dst.Steam.GetCdnServersAsync(100, null, cancellationToken);
        var servers3 = await dst.Steam.GetCdnServersAsync(150, null, cancellationToken);
        var servers4 = await dst.Steam.GetCdnServersAsync(200, null, cancellationToken);
        IEnumerable<SteamContentServer> servers = [.. servers1, .. servers2, .. servers3, .. servers4];
        var stableServers = await SteamHelper.TestContentServerConnectionAsync(dst.Steam.HttpClient, servers, TimeSpan.FromSeconds(4));
        dst.Steam.ContentServers = stableServers.ToList();
    }
}
