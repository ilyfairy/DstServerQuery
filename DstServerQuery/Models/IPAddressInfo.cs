using MaxMind.GeoIP2.Model;
using System.Net;
using System.Text.Json.Serialization;

namespace DstServerQuery.Models;

public record class IPAddressInfo
{
    public string IP => IPAddress.ToString();

    /// <summary>
    /// 两位数的ISO代码
    /// </summary>
    public string? IsoCode => CountryInfo?.IsoCode;

    [JsonIgnore]
    public Country? CountryInfo { get; init; }

    [JsonIgnore]
    public City? CityInfo { get; init; }

    [JsonIgnore]
    public IPAddress IPAddress { get; init; }
}
