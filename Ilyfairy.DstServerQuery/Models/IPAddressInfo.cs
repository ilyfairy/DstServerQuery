using MaxMind.GeoIP2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;

public class IPAddressInfo
{
    public string IP { get; set; }

    public Country? Country { get; set; }

}
