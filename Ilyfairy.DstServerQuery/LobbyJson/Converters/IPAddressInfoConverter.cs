using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;


public abstract class IPAddressReadConverter : JsonConverter<IPAddressInfo>
{
    public GeoIPService? GeoIPService { get; }
    private readonly IPAddressInfo local = new()
    {
        CountryInfo = null,
        IPAddress = IPAddress.Loopback,
    };

    public IPAddressReadConverter(GeoIPService? geoIPService)
    {
        GeoIPService = geoIPService;
    }

    public override IPAddressInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            _ = JsonNode.Parse(ref reader);
            return null;
        }

        string? ip = reader.GetString();
        if (reader.TokenType != JsonTokenType.String || string.IsNullOrWhiteSpace(ip)) return null;

        ip = ip.Trim();
        if (ip == "127.0.0.1") return local;
        
        IPAddressInfo info;
        IPAddress? ipAddress = null;
        try
        {
            ipAddress = IPAddress.Parse(ip);
            if (GeoIPService?.TryCity(ip, out var city) == true)
            {
                info = new IPAddressInfo()
                {
                    IPAddress = ipAddress,
                    CountryInfo = city?.Country,
                    CityInfo = city?.City,
                };
            }
            else
            {
                info = new IPAddressInfo()
                {
                    IPAddress = ipAddress,
                };
            }
        }
        catch
        {
            info = new IPAddressInfo()
            {
                CountryInfo = null,
                IPAddress = ipAddress,
            };
        }

        return info;
    }
}

public class IPAddressStringConverter : IPAddressReadConverter
{
    public IPAddressStringConverter(GeoIPService geoIPService) : base(geoIPService)
    {
    }

    public IPAddressStringConverter() : base(null)
    {
        
    }

    public override void Write(Utf8JsonWriter writer, IPAddressInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.IP);
    }
}


public class IPAddressInfoConverter : IPAddressReadConverter
{
    public IPAddressInfoConverter(GeoIPService? geoIPService) : base(geoIPService)
    {
    }

    public override void Write(Utf8JsonWriter writer, IPAddressInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
        //var r = JsonSerializer.Serialize(value, options);
        //writer.WriteStartObject();
        //writer.WriteString("IP", value.IP);
        //writer.WriteString("CountryCode", value.CountryCode);
        //writer.WriteEndObject();
    }
}
