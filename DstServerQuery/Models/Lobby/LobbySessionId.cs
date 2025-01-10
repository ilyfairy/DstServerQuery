using System.Diagnostics;
using System.Text.Json.Serialization;
using DstServerQuery.Converters;

namespace DstServerQuery.Models.Lobby;

// "0E516F29EF6D3712"
// "05375D373EC81FY3"
[JsonConverter(typeof(LobbySessionIdConverter))]
[DebuggerDisplay("{ToString()}")]
public readonly struct LobbySessionId
{
    public readonly byte[] Value { get; }

    public LobbySessionId(ReadOnlySpan<byte> sessionId)
    {
        if (sessionId.Length != 16)
        {
            throw new Exception("Invalid session id length");
        }

        foreach (var c in sessionId)
        {
            if (c > 127)
            {
                throw new Exception("Invalid session id character");
            }
        }
        Value = new byte[16];
        sessionId.CopyTo(Value);
    }

    public override string ToString()
    {
        return string.Create(16, Value, (span, value) =>
        {
            for (var i = 0; i < 16; i++)
            {
                span[i] = (char)value[i];
            }
        });
    }
}
