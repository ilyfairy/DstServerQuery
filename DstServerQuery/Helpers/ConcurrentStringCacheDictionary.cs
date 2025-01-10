using System.Buffers;
using System.Collections.Concurrent;
using System.Text;

namespace DstServerQuery.Helpers;

public class ConcurrentStringCacheDictionary
{
    public ConcurrentDictionary<string, string> Dictionary { get; } = new();
    private readonly ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> _cacheAlternateLookup;

    public ConcurrentStringCacheDictionary()
    {
        _cacheAlternateLookup = Dictionary.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public string GetOrAdd(ReadOnlySpan<char> chars)
    {
        if (_cacheAlternateLookup.TryGetValue(chars, out var str))
        {
            return str;
        }
        else
        {
            str = chars.ToString();
            Dictionary.TryAdd(str, str);
            return str;
        }
    }

    public string GetOrAdd(ReadOnlySpan<byte> bytes)
    {
        var charLen = Encoding.UTF8.GetCharCount(bytes);
        if (charLen < 512)
        {
            Span<char> buffer = stackalloc char[charLen];
            var writenLen = Encoding.UTF8.GetChars(bytes, buffer);
            if (_cacheAlternateLookup.TryGetValue(buffer[0..writenLen], out var tags))
            {
                return tags;
            }
            else
            {
                tags = buffer[0..writenLen].ToString();
                _cacheAlternateLookup.TryAdd(buffer[0..writenLen], buffer[0..writenLen].ToString());
                return tags;
            }
        }
        else
        {
            var buffer = ArrayPool<char>.Shared.Rent(charLen);
            try
            {
                var writenLen = Encoding.UTF8.GetChars(bytes, buffer);
                if (_cacheAlternateLookup.TryGetValue(buffer.AsSpan()[0..writenLen], out var tags))
                {
                    return tags;
                }
                else
                {
                    tags = buffer.AsSpan()[0..writenLen].ToString();
                    _cacheAlternateLookup.TryAdd(buffer.AsSpan()[0..writenLen], buffer.AsSpan()[0..writenLen].ToString());
                    return tags;
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }
}
