using Ilyfairy.DstServerQuery.Models.LobbyData;
using MaxMind.GeoIP2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Utils;

public static class GeoIPManager
{
    public static DatabaseReader? GeoIP { get; private set; }

    public static void Initialize(string GeoLite2Path = "GeoLite2-City.mmdb")
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

}
