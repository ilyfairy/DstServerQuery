using Ilyfairy.DstServerQuery.Models.LobbyData.Units;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Helpers.Converters;

public class IntentWriteConverter : JsonConverter<IntentionType>
{
    public override IntentionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IntentionType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class IntentWithTranslateConverter : JsonConverter<IntentionType>
{
    public override IntentionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IntentionType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DstEnumText.Instance.GetFromString(value.Value ?? "", value.Value ?? ""));
    }
}