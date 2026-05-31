using SteamDownloader.Helpers.JsonConverters;
using System.Text.Json;

namespace SteamDownloader.WebApi.Interfaces;

public abstract class InterfaceBase(SteamSession steamSession)
{
    public string ApiKey => steamSession.SteamClient.Configuration.WebAPIKey;
    public Uri WebApiBaseAddress => steamSession.SteamClient.Configuration.WebAPIBaseAddress;

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new DateTimeOffsetSecondConverter(),
            new NumberToBooleanConverter(),
        },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
