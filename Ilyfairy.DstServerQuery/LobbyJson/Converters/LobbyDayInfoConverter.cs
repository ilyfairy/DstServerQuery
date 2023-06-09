﻿using Ilyfairy.DstServerQuery.Models;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converter;

public partial class LobbyDayInfoConverter : JsonConverter<LobbyDayInfo>
{
    
    public override LobbyDayInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dayLua;
        try
        {
            dayLua = JsonNode.Parse(ref reader)?.GetValue<string?>();
            if (dayLua == null) return null;
        }
        catch (Exception)
        {
            return null;
        }

        var match = DayRegex().Match(dayLua);
        if (match.Success)
        {
            LobbyDayInfo info = new();
            info.Day = int.Parse(match.Groups[1].Value);
            info.DaysElapsedInSeason = int.Parse(match.Groups[2].Value);
            info.DaysLeftInSeason = int.Parse(match.Groups[3].Value);
            return info;
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, LobbyDayInfo value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    [GeneratedRegex("return \\{ day=(\\d+), dayselapsedinseason=(\\d+), daysleftinseason=(\\d+) \\}")]
    private static partial Regex DayRegex();
}
