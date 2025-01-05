using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DstServerQuery.Services;

public class GeoIPService
{
    private readonly ILogger<GeoIPService>? _logger;

    public DatabaseReader? GeoIP { get; private set; }

    public GeoIPService(ILogger<GeoIPService>? logger = null)
    {
        this._logger = logger;
    }

    public void Initialize(string GeoLite2Path = "GeoLite2-City.mmdb")
    {
        try
        {
            GeoIP = new DatabaseReader(GeoLite2Path, MaxMind.Db.FileAccessMode.MemoryMapped);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GeoIP初始化异常");
        }
    }

    public bool TryCity(string ip, out CityResponse? city)
    {
        city = null;
        if (GeoIP is null) return false;
        return GeoIP.TryCity(ip, out city);
    }

    public bool TryCity(IPAddress ip, out CityResponse? city)
    {
        city = null;
        if (GeoIP is null) return false;
        return GeoIP.TryCity(ip, out city);
    }

}
