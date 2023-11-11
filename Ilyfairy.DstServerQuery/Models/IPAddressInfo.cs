using MaxMind.GeoIP2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;

public record class IPAddressInfo
{
    public string IP { get; set; }

    /// <summary>
    /// 两位数的ISO代码
    /// </summary>
    public string? IsoCode => CountryInfo?.IsoCode;

    [JsonIgnore]
    public Country? CountryInfo { get; set; }

    public IPAddressInfo(string ip)
    {
        IP = ip;
    }
}
