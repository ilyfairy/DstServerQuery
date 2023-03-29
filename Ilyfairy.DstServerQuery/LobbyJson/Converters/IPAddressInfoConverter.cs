using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class IPAddressInfoConverter : JsonConverter<IPAddressInfo>
{
    public override IPAddressInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            _ = JsonNode.Parse(ref reader);
            return null;
        }

        IPAddressInfo info = new();
        string? ip = reader.GetString();
        if (reader.TokenType != JsonTokenType.String || string.IsNullOrWhiteSpace(ip)) return null;

        info.IP = ip.Trim();
        try
        {
            if (GeoIPManager.GeoIP?.TryCity(info.IP, out var city) == true)
            {
                info.Country = city?.Country;
            }
        }
        catch (Exception)
        {
        }

        return info;
    }

    public override void Write(Utf8JsonWriter writer, IPAddressInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.IP);
    }
}
