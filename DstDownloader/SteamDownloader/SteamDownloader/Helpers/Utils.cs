using System.Text;

namespace SteamDownloader.Helpers;

internal static class Utils
{
    public static string MakeQueryParams(params KeyValuePair<string, string?>[] keyValuePairs)
    {
        StringBuilder sb = new(keyValuePairs.Sum(v => v.Key.Length + v.Value?.Length ?? 0 + 2));
        foreach (var (key, value) in keyValuePairs)
        {
            if (value is null)
                continue;

            sb.Append(key);
            sb.Append('=');
            sb.Append(Uri.UnescapeDataString(value));
            sb.Append('&');
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
}
