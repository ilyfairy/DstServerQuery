using System.Text.Json.Serialization;
using DstServerQuery.Converters;

namespace DstServerQuery.Models.Lobby;

[JsonConverter(typeof(LobbyNumberIdConverter))]
public readonly struct LobbyNumberId // "Id" "123456"
{
    public readonly long Value { get; }

    public LobbyNumberId(long value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
