using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using Ilyfairy.DstServerQuery.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.LobbyJson.Converters;

public class SeasonConverter : JsonConverter<Season>
{
    public override Season Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new Season(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Season value, JsonSerializerOptions options)
    {
        //writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value, "未知"));
        writer.WriteStringValue(value.Value);
    }
}

public class SeasonWithTranslateConverter : SeasonConverter
{
    public override void Write(Utf8JsonWriter writer, Season value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}