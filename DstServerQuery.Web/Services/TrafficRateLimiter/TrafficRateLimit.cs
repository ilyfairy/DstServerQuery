using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery.Web.Services.TrafficRateLimiter;

/// <summary>
/// 在<see cref="Window"/>秒内, 只能请求<see cref="TrafficBytes"/>字节
/// </summary>
public partial class TrafficRateLimit
{
    private string window = null!;
    public string Window
    {
        get => window;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            var match = ValueRegex().Match(value);
            if (!match.Success)
                throw new ArgumentException(null, nameof(value));

            double v = double.Parse(match.Groups["value"].Value);
            string unit = match.Groups["unit"].Value;

            WindowSec = (int)(unit switch
            {
                "s" or "sec" => v,
                "m" or "minute" => v * 60,
                "h" or "hour" => v * 3600,
                "d" or "day" or "days" => v * 3600 * 24,
                _ => v,
            });
            window = value;
        }
    }


    public int WindowSec { get; private set; }

    private string traffic = null!;

    public required string Traffic
    {
        get => traffic;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            var match = ValueRegex().Match(value);
            if (!match.Success)
                throw new ArgumentException(null, nameof(value));

            double v = double.Parse(match.Groups["value"].Value);
            string unit = match.Groups["unit"].Value;

            TrafficBytes = (int)(unit switch
            {
                "b" or "byte" or "bytes" => v,
                "kb" or "k" => v * 1024,
                "mb" or "m" => v * 1024 * 1024,
                _ => v,
            });

            traffic = value;
        }
    }

    public int TrafficBytes { get; private set; }

    [GeneratedRegex(@"^(?<value>[0-9\.]+)(?<unit>.*)$")]
    private static partial Regex ValueRegex();
}