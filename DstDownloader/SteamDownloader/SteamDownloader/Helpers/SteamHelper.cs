namespace SteamDownloader.Helpers;

public static class SteamHelper
{
    public static async Task<SteamContentServer[]> TestContentServerConnectionAsync(HttpClient httpClient, IEnumerable<SteamContentServer> servers, TimeSpan timeout)
    {
        List<SteamContentServer> success = new();
        await Parallel.ForEachAsync(servers, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, async (s, _) =>
        {
            CancellationTokenSource cts = new();
            cts.CancelAfter(timeout);
            Uri url = new UriBuilder("https", s.Host).Uri;
            try
            {
                var response = await httpClient.GetAsync(url, cts.Token);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return;
            }
            if(cts.IsCancellationRequested)
            {
                return;
            }

            lock (success)
            {
                success.Add(s);
            }
        }).ConfigureAwait(false);
        return success.ToArray();
    }
}