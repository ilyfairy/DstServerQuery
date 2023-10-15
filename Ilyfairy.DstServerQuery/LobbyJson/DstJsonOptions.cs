using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson;

public class DstJsonOptions
{
    public DstJsonOptions()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        SerializerOptions.Converters.Add(new DateTimeJsonConverter());
        SerializerOptions.Converters.Add(new ModeConverter());
        SerializerOptions.Converters.Add(new IntentConverter());
        SerializerOptions.Converters.Add(new SeasonConverter());
        SerializerOptions.Converters.Add(new CustomNameConverter<LobbyBriefsData>());
        SerializerOptions.Converters.Add(new CustomNameConverter<LobbyBriefsDataPlayers>());
        SerializerOptions.Converters.Add(new CustomNameConverter<LobbyDetailsData>());

        DeserializerOptions.Converters.Add(new JsonStringEnumConverter());
        SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    public JsonSerializerOptions SerializerOptions { get; private set; } = new();
    public JsonSerializerOptions DeserializerOptions { private set; get; } = new();

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