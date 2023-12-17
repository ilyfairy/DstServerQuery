using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using Ilyfairy.DstServerQuery.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters;

public class IntentConverter : JsonConverter<IntentionType>
{
    public override IntentionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new IntentionType(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, IntentionType value, JsonSerializerOptions options)
    {
        //writer.WriteStringValue(DstEnumText.Instance.TryGetValueOrDefault(value.Value, "未知"));
        writer.WriteStringValue(value.Value);
    }
}

public class IntentWithTranslateConverter : IntentConverter
{
    public override void Write(Utf8JsonWriter writer, IntentionType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}