using System.Buffers;
using System.Text;
using System.Text.Json;

namespace DstServerQuery.Helpers;

public ref struct TempUtf8JsonString : IDisposable
{
    private char[]? _sharedBuffer;
    public ReadOnlyMemory<char> String { get; private set; }

    public static TempUtf8JsonString From(Utf8JsonReader reader)
    {
        var charCount = Encoding.UTF8.GetCharCount(reader.ValueSpan);
        var str = new TempUtf8JsonString()
        {
            _sharedBuffer = ArrayPool<char>.Shared.Rent(charCount)
        };
        
        var writenLen = reader.CopyString(str._sharedBuffer);
        str.String = new ReadOnlyMemory<char>(str._sharedBuffer, 0, writenLen);
        return str;
    }

    public void Dispose()
    {
        if (_sharedBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_sharedBuffer);
            _sharedBuffer = null;
            String = default;
        }
    }

    public override readonly string ToString() => String.ToString();
}
