using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson;

public static class DstJsonConverter
{
    static DstJsonConverter()
    {
        Options = new();
        Options.Converters.Add(new DateTimeJsonConverter());
        Options.Converters.Add(new ModeConverter());
        Options.Converters.Add(new IntentConverter());
        Options.Converters.Add(new SeasonConverter());
        Options.Converters.Add(new JsonStringEnumConverter());
        Options.Converters.Add(new CustomNameConverter<LobbyBriefData>());
        Options.Converters.Add(new CustomNameConverter<LobbyBriefDataPlayers>());
        Options.Converters.Add(new CustomNameConverter<LobbyDetailsData>());
        Options.Converters.Add(new IPAddressInfoConverter());
        Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    public static JsonSerializerOptions Options { private set; get; }

}

public class ModeConverter : JsonConverter<GameMode>
{
    public override GameMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, GameMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value));
    }
}

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _dateFormatString;
    public DateTimeJsonConverter() => _dateFormatString = "yyyy-MM-dd HH:mm:ss";

    public DateTimeJsonConverter(string dateFormatString) => _dateFormatString = dateFormatString;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(_dateFormatString));
    }
}


public class IntentConverter : JsonConverter<IntentionType>
{
    public override IntentionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IntentionType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value, "未知"));
    }
}

public class SeasonConverter : JsonConverter<Season>
{
    public override Season Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Season value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value, "未知"));
    }
}

public class PlatformConverter : JsonConverter<Platform>
{
    public override Platform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Platform value, JsonSerializerOptions options)
    {
        if (value == (Platform)19)
        {
            writer.WriteStringValue("PS4 Official");
        }
        else
        {
            var str = DstEnumText.Instance.TryGetValueOrDefault(value, "未知");
            writer.WriteStringValue(str);
        }
    }
}