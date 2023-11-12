using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;


public abstract class IPAddressReadConverter : JsonConverter<IPAddressInfo>
{
    public GeoIPService? GeoIPService { get; }

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

        IPAddressInfo info = new(ip.Trim());
        if (ip == "127.0.0.1") return info;

        try
        {
            if (GeoIPService?.TryCity(info.IP, out var city) == true)
            {
                info.CountryInfo = city?.Country;
            }
        }
        catch (Exception)
        {
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
