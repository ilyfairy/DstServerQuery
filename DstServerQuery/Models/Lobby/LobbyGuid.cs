using System.Diagnostics;
using System.Text.Json.Serialization;
using DstServerQuery.Converters;

namespace DstServerQuery.Models.Lobby;

[JsonConverter(typeof(LobbyGuidConverter))]
public readonly struct LobbyGuid
{
    public readonly ulong Value { get; }

    public LobbyGuid(ReadOnlySpan<byte> id)
    {
        try
        {
            Value = ulong.Parse(id);
        }
        catch (Exception)
        {
            throw new Exception("Invalid steam id character");
        }
    }

    public override string ToString() => Value.ToString();
}
