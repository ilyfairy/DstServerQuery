namespace SteamDownloader.Helpers;

public static class SteamHelper
{
    public static async Task<SteamContentServer[]> TestContentServerConnectionAsync(HttpClient httpClient, IEnumerable<SteamContentServer> servers, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        List<SteamContentServer> success = new();
        await Parallel.ForEachAsync(servers, new ParallelOptions() { MaxDegreeOfParallelism = 8, CancellationToken = cancellationToken }, async (s, token) =>
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(timeout);
            try
            {
                using var response = await httpClient.GetAsync(s.Url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
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
