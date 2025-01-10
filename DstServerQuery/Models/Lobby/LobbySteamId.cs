using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using DstServerQuery.Converters;

namespace DstServerQuery.Models.Lobby;

[DebuggerDisplay("{ToString()}")]
[JsonConverter(typeof(LobbySteamIdConverter))]
public readonly struct LobbySteamId
{
    public readonly ulong Value { get; }
    public string? String { get; }

    public LobbySteamId(ReadOnlySpan<byte> id)
    {
        Span<char> number = stackalloc char[id.Length];
        for (var i = 0; i < id.Length; i++)
        {
            var c = (char)id[i];
            if (!char.IsNumber(c))
            {
                String = Encoding.UTF8.GetString(id);
                return;
            }
            number[i] = c;
        }
        Value = ulong.Parse(number);
    }

    public override string ToString()
    {
        if (String is not null)
        {
            return String;
        }
        else
        {
            return Value.ToString();
        }
    }

    public bool Equals(ReadOnlySpan<char> str, bool isIgnoreCase = false)
    {
        if (String is not null)
        {
            if (isIgnoreCase)
            {
                return String.AsSpan().Equals(str, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return String == str;
            }
        }
        else
        {
            if (ulong.TryParse(str, out var number))
            {
                return Value == number;
            }
            else
            {
                return false;
            }
        }

    }
}
