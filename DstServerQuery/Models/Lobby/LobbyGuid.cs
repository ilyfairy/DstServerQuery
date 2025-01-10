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
        Span<char> number = stackalloc char[id.Length];
        for (var i = 0; i < id.Length; i++)
        {
            var c = (char)id[i];
            if (!char.IsNumber(c))
            {
                throw new Exception("Invalid steam id character");
            }
            number[i] = c;
        }
        try
        {
            Value = ulong.Parse(number);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public override string ToString() => Value.ToString();
}
