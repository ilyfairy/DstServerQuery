using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using Ilyfairy.DstServerQuery.Utils;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converters;

public class ModeConverter : JsonConverter<GameMode>
{
    public override GameMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var mode = reader.GetString();
        return new GameMode(mode);
    }

    public override void Write(Utf8JsonWriter writer, GameMode value, JsonSerializerOptions options)
    {
        //writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value));
        writer.WriteStringValue(value.Value);
    }
}

public class ModeWithTranslateConverter : ModeConverter
{
    public override void Write(Utf8JsonWriter writer, GameMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}