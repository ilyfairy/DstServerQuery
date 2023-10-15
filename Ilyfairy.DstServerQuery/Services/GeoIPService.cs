using Ilyfairy.DstServerQuery.Models.LobbyData;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Services;

public class GeoIPService
{
    public DatabaseReader? GeoIP { get; private set; }

    public void Initialize(string GeoLite2Path = "GeoLite2-City.mmdb")
    {
        try
        {
            GeoIP = new DatabaseReader(GeoLite2Path, MaxMind.Db.FileAccessMode.Memory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public bool TryCity(string ip, out CityResponse? city)
    {
        city = null;
        if (GeoIP is null) return false;
        return GeoIP.TryCity(ip, out city);
    }

}
