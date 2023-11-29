using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;

namespace Ilyfairy.DstServerQuery.Web.Services.TrafficRateLimiter;

public class TrafficMonitorStream(IHttpResponseBodyFeature httpResponseBodyFeature) : Stream, IHttpResponseBodyFeature
{
    public int Bytes { get; set; }

    public override bool CanRead => BaseStream.CanRead;

    public override bool CanSeek => BaseStream.CanSeek;

    public override bool CanWrite => BaseStream.CanWrite;

    public override long Length => BaseStream.Length;

    public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

    public Stream BaseStream { get; set; } = httpResponseBodyFeature.Stream;

    public Stream Stream => this;

    private PipeWriter? _pipeAdapter;

    public PipeWriter Writer
    {
        get
        {
            _pipeAdapter ??= PipeWriter.Create(Stream, new StreamPipeWriterOptions(leaveOpen: true));
            return _pipeAdapter;
        }
    }

    public Task CompleteAsync() => httpResponseBodyFeature.CompleteAsync();

    public void DisableBuffering() => httpResponseBodyFeature.DisableBuffering();

    public override void Flush() => BaseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
    {
        return httpResponseBodyFeature.SendFileAsync(path, offset, count, cancellationToken);
    }

    public override void SetLength(long value) => BaseStream.SetLength(value);

    public Task StartAsync(CancellationToken cancellationToken = default) => httpResponseBodyFeature.StartAsync(cancellationToken);

    public override void Write(byte[] buffer, int offset, int count)
    {
        Bytes += count;
        BaseStream.Write(buffer, offset, count);
    }


    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Bytes += count;
        await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        Bytes += buffer.Length;
        await BaseStream.WriteAsync(buffer, cancellationToken);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Bytes += buffer.Length;
        BaseStream.Write(buffer);
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        Bytes += count;
        return BaseStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        Bytes += count;
        return BaseStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        BaseStream.CopyTo(destination, bufferSize);
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);

    public override void WriteByte(byte value)
    {
        Bytes += value;
        BaseStream.WriteByte(value);
    }

    public override void EndWrite(IAsyncResult asyncResult) => BaseStream.EndWrite(asyncResult);
}