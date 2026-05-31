using SteamKit2;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SteamDownloader.Helpers;

public static class SteamSessionExtensions
{
    public static async Task<byte[]> DownloadChunkDataWithRetryAsync(this SteamSession steamSession, uint depotId, DepotManifest.ChunkData chunkData, byte[] depotKey, int retry = 3, CancellationToken cancellationToken = default)
    {
        if (retry < 0)
            retry = 1;
        Exception exception = null!;
        for (int i = 0; i < retry; i++)
        {
            try
            {
                return await steamSession.DownloadChunkDataAsync(depotId, chunkData, depotKey, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw;
                exception = e;
            }
        }
        throw exception;
    }

    public static async Task DownloadFileDataToStreamAsync(this SteamSession steamSession, Stream stream, uint appId, uint depotId, DepotManifest.FileData fileData, CancellationToken cancellationToken = default)
    {
        var depotKey = await steamSession.GetDepotKeyAsync(appId, depotId).ConfigureAwait(false);
        await DownloadFileDataToStreamAsync(steamSession, stream, depotId, depotKey, fileData, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<byte[]> DownloadFileDataBytesAsync(this SteamSession steamSession, uint depotId, byte[] depotKey, DepotManifest.FileData fileData, CancellationToken cancellationToken = default)
    {
        MemoryStream ms = new((int)fileData.TotalSize);
        await DownloadFileDataToStreamAsync(steamSession, ms, depotId, depotKey, fileData, cancellationToken).ConfigureAwait(false);

        if(ms.Length == ms.Capacity)
        {
            return ms.GetBuffer();
        }
        else
        {
            return ms.ToArray(); // fileData.TotalSize和实际大小不一致
        }
    }

    public static async Task DownloadFileDataToStreamAsync(this SteamSession steamSession, Stream stream, uint depotId, byte[] depotKey, DepotManifest.FileData fileData, CancellationToken cancellationToken = default)
    {
        if (fileData.Flags.HasFlag(EDepotFileFlag.Directory))
            throw new Exception("FileData不是一个文件");

        if (!stream.CanWrite)
            throw new Exception("流无法写入");

        if (fileData.Chunks.Count == 1)
        {
            var data = await steamSession.DownloadChunkDataWithRetryAsync(depotId, fileData.Chunks.First(), depotKey, 5, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (stream.CanSeek)
        {
            var startOffset = stream.Position;

            using SemaphoreSlim writeLock = new(1);
            var opt = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 8,
            };
            await Parallel.ForEachAsync(fileData.Chunks, opt, async (chunk, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var data = await steamSession.DownloadChunkDataWithRetryAsync(depotId, chunk, depotKey, 5, cancellationToken).ConfigureAwait(false);

                try
                {
                    await writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    stream.Position = startOffset + (long)chunk.Offset;
                    await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    writeLock.Release();
                }
            });
        }
        else
        {
            foreach (var item in fileData.Chunks.OrderBy(v => v.Offset))
            {
                var data = await steamSession.DownloadChunkDataWithRetryAsync(depotId, item, depotKey, 5, cancellationToken).ConfigureAwait(false);
                await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public static async Task DownloadFileDataToDirectoryAsync(this SteamSession steamSession, string dir, uint appId, uint depotId, DepotManifest.FileData fileData, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(dir, fileData.FileName);
        if (fileData.Flags.HasFlag(EDepotFileFlag.Directory))
        {
            Directory.CreateDirectory(path);
            return;
        }

        Directory.CreateDirectory(dir);
        using FileStream fs = new(path, FileMode.OpenOrCreate);

        if ((long)fileData.TotalSize == fs.Length)
        {
            var fileSHA1 = await SHA1.HashDataAsync(fs, cancellationToken).ConfigureAwait(false);
            if (fileData.FileHash.SequenceEqual(fileSHA1))
            {
                return;
            }
            fs.Seek(0, SeekOrigin.Begin);
        }

        await steamSession.DownloadFileDataToStreamAsync(fs, appId, depotId, fileData, cancellationToken).ConfigureAwait(false);
    }

    public static async Task DownloadDepotManifestToDirectoryAsync(this SteamSession steamSession, string dir, uint appId, DepotManifest depotManifest, CancellationToken cancellationToken = default)
    {
        var depotKey = await steamSession.GetDepotKeyAsync(appId, depotManifest.DepotID);
        await DownloadDepotManifestToDirectoryAsync(steamSession, dir, depotKey, depotManifest, cancellationToken).ConfigureAwait(false);
    }

    public static Task DownloadDepotManifestToDirectoryAsync(this SteamSession steamSession, string dir, byte[] depotKey, DepotManifest depotManifest, [StringSyntax(StringSyntaxAttribute.Regex)] string pathSearchRegex, CancellationToken cancellationToken = default)
    {
        if (depotManifest.FilenamesEncrypted)
            throw new Exception("DepotManifest没有解密");

        if (depotManifest.Files is null)
            throw new Exception("DepotManifest.Files为null");

        return DownloadDepotManifestToDirectoryAsync(steamSession, dir, depotManifest.DepotID, depotKey, (depotManifest.Files ?? []).Where(v => Regex.IsMatch(v.FileName, pathSearchRegex)), cancellationToken);
    }

    public static Task DownloadDepotManifestToDirectoryAsync(this SteamSession steamSession, string dir, byte[] depotKey, DepotManifest depotManifest, CancellationToken cancellationToken = default)
    {
        if (depotManifest.FilenamesEncrypted)
            throw new Exception("DepotManifest没有解密");

        if (depotManifest.Files is null)
            throw new Exception("DepotManifest.Files为null");

        return DownloadDepotManifestToDirectoryAsync(steamSession, dir, depotManifest.DepotID, depotKey, depotManifest.Files ?? [], cancellationToken);
    }

    public static async Task DownloadDepotManifestToDirectoryAsync(this SteamSession steamSession, string dir, uint depotId, byte[] depotKey, IEnumerable<DepotManifest.FileData> manifestFiles, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(dir);

        var files = new List<DepotManifest.FileData>();

        HashSet<string> dirs = new();
        foreach (var item in manifestFiles)
        {
            if(item.Flags.HasFlag(EDepotFileFlag.Directory))
            {
                Directory.CreateDirectory(Path.Combine(dir, item.FileName));
            }
            else
            {
                files.Add(item);
            }
        }

        if (files.Count == 0)
            return;

        if(files.Count == 1)
        {
            var file = files.First();
            var fullPath = Path.Combine(dir, file.FileName);

            await DownloadAsync(steamSession, fullPath, file, depotId, depotKey, cancellationToken);
        }

        if (files.Sum(v => (long)v.TotalSize) <= 10 * 1024 * 1024)
        {
            var tasks = files.Select(v =>
            {
                var file = files.First();
                var fullPath = Path.Combine(dir, file.FileName);
                return DownloadAsync(steamSession, fullPath, file, depotId, depotKey, cancellationToken);
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        const int _1mb = 1;
        const int _10mb = 2;
        const int _max = 2;
        var sizeGroup = files.OrderByDescending(v => v.TotalSize).GroupBy(v => v.TotalSize switch
        {
            <= 1024 * 1024 => _1mb,
            <= 10 * 1024 * 1024 => _10mb,
            _ => _max,
        });
        var sFiles = sizeGroup.FirstOrDefault(v => v.Key is _1mb);
        var lFiles = sizeGroup.FirstOrDefault(v => v.Key is _10mb);
        var maxFiles = sizeGroup.FirstOrDefault(v => v.Key is _max);

        if (maxFiles is { })
            await ParallelForEachAsync(maxFiles, 1).ConfigureAwait(false);
        if (lFiles is { })
            await ParallelForEachAsync(lFiles, 3).ConfigureAwait(false);
        if (sFiles is { })
            await ParallelForEachAsync(sFiles, 10).ConfigureAwait(false);

        return;

        Task ParallelForEachAsync(IEnumerable<DepotManifest.FileData> fileDatas, int maxDegreeOfParallelism)
        {
            var opt = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            };
            return Parallel.ForEachAsync(fileDatas, opt, (fileData, cancellationToken) =>
            {
                var fullPath = Path.Combine(dir, fileData.FileName);

                return new ValueTask(DownloadAsync(steamSession, fullPath, fileData, depotId, depotKey, cancellationToken));
            });
        }

        static Task DownloadAsync(SteamSession steamSession, string fullPath, DepotManifest.FileData fileData, uint depotId, byte[] depotKey, CancellationToken cancellationToken)
        {
            try
            {
                return Down();
            }
            catch (DirectoryNotFoundException)
            {
                var d = Path.GetDirectoryName(fullPath)!;
                if (!Directory.Exists(d))
                    Directory.CreateDirectory(d);

                return Down();
            }

            async Task Down()
            {
                using FileStream fs = new(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                if(fs.Length == (long)fileData.TotalSize)
                {
                    if (fileData.FileHash.SequenceEqual(await SHA1.HashDataAsync(fs, cancellationToken).ConfigureAwait(false)))
                        return;
                }

                fs.SetLength((long)fileData.TotalSize);
                await steamSession.DownloadFileDataToStreamAsync(fs, depotId, depotKey, fileData, cancellationToken).ConfigureAwait(false);
            }

        }
    }
}
