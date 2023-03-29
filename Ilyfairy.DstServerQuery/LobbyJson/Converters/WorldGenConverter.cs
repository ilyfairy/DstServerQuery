using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Utils;
using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public class WorldGenConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        _ = JsonNode.Parse(ref reader);
        return null;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
