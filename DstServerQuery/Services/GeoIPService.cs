using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DstServerQuery.Services;

public class GeoIPService
{
    public DatabaseReader? GeoIP { get; private set; }

    public void Initialize(string GeoLite2Path = "GeoLite2-City.mmdb")
    {
        GeoIP = new DatabaseReader(GeoLite2Path, MaxMind.Db.FileAccessMode.MemoryMapped);
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
