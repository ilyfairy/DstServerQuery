using Ilyfairy.DstServerQuery.Helpers.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Ilyfairy.DstServerQuery.Helpers;

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
        SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        DeserializerOptions.Converters.Add(new JsonStringEnumConverter());
        DeserializerOptions.Converters.Add(new PlatformConverter());
        //SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    public JsonSerializerOptions SerializerOptions { get; private set; } = new();
    public JsonSerializerOptions DeserializerOptions { private set; get; } = new();
}
