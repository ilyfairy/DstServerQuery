using System.Net;

namespace DstServerQuery.Helpers;

public class StreamWriterHttpContent(Func<Stream, Task> streamCallabck) : HttpContent
{
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return streamCallabck?.Invoke(stream) ?? Task.CompletedTask;
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}