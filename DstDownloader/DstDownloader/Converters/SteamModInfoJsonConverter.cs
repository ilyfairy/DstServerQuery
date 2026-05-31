using System.Text.Json;
using System.Text.Json.Serialization;
using DstDownloaders.Mods;
using SteamDownloader.WebApi;
using SteamDownloader.WebApi.Interfaces;

namespace DstDownloaders.Converters;

public class SteamModInfoJsonConverter : JsonConverter<SteamModInfo>
{
    public override SteamModInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        WorkshopFileDetails? workshopFileDetails = JsonSerializer.Deserialize<WorkshopFileDetails>(ref reader, InterfaceBase.JsonOptions);

        if (workshopFileDetails is null)
            return null;

        return new SteamModInfo(workshopFileDetails);
    }

    public override void Write(Utf8JsonWriter writer, SteamModInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.details, InterfaceBase.JsonOptions);
    }
}