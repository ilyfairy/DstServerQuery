using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Ilyfairy.DstServerQuery.LobbyJson;

public class DstJsonOptions
{
    public DstJsonOptions()
    {
        //SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        SerializerOptions.Converters.Add(new PlatformConverter());
        //SerializerOptions.Converters.Add(new DateTimeJsonConverter());
        //SerializerOptions.Converters.Add(new CustomNameConverter<LobbyBriefsData>());
        //SerializerOptions.Converters.Add(new CustomNameConverter<LobbyBriefsDataPlayers>());
        //SerializerOptions.Converters.Add(new CustomNameConverter<LobbyDetailsData>());
        SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();

        DeserializerOptions.Converters.Add(new JsonStringEnumConverter());
        DeserializerOptions.Converters.Add(new PlatformConverter());
        //SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    public JsonSerializerOptions SerializerOptions { get; private set; } = new();
    public JsonSerializerOptions DeserializerOptions { private set; get; } = new();
}

//public class DateTimeJsonConverter : JsonConverter<DateTime>
//{
//    private readonly string _dateFormatString;
//    public DateTimeJsonConverter() => _dateFormatString = "yyyy-MM-dd HH:mm:ss";

//    public DateTimeJsonConverter(string dateFormatString) => _dateFormatString = dateFormatString;

//    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        return DateTime.Parse(reader.GetString()!);
//    }

//    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
//    {
//        var val = value.ToUniversalTime().ToString(_dateFormatString);
//        writer.WriteStringValue(val);
//    }
//}
