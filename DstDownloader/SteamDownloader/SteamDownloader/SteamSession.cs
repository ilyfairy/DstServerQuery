using SteamDownloader.WebApi;
using SteamDownloader.WebApi.Interfaces;
using SteamKit2;
using SteamKit2.CDN;
using SteamKit2.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks.Dataflow;

namespace SteamDownloader;

public partial class SteamSession : IDisposable
{
    public HttpClient HttpClient { get; set; }
    public SteamClient SteamClient { get; }
    public CallbackManager CallbackManager { get; }

    private readonly SteamUser _steamUser;
    private readonly SteamApps _steamApps;
    private readonly SteamContent _steamContent;
    private readonly SteamCloud _steamCloud;
    private readonly PublishedFile _publishedFile;

    public bool IsCache { get; set; } = true;
    private readonly ConcurrentDictionary<uint, ulong> _appTokensCache = new();
    private readonly ConcurrentDictionary<uint, SteamApps.PICSProductInfoCallback.PICSProductInfo> _appInfosCache = new();
    private readonly ConcurrentDictionary<uint, byte[]> _depotKeysCache = new();

    public PublishedFileService PublishedFileService { get; }
    public SteamRemoteStorage SteamRemoteStorage { get; }

    public SteamAuthentication Authentication { get; }

    public event EventHandler<SteamClient.DisconnectedCallback>? Disconnected;
    private EResult _connectionLoginResult;

    private readonly SemaphoreSlim _loginLock = new(1);
    private readonly object _contentServersLock = new();

    public List<SteamContentServer> ContentServers { get; set; } = new();

    private readonly BufferBlock<CallbackMsg> _steamClientCallbackQueue;

    private static readonly FieldInfo? _steamClientCallbackQueueFieldInfo;

    static SteamSession()
    {
        _steamClientCallbackQueueFieldInfo = typeof(SteamClient).GetField("callbackQueue", ~BindingFlags.Default)!;
    }

    public SteamSession(SteamConfiguration? steamConfiguration = null)
    {
        if (steamConfiguration is null)
        {
            SteamClient = new();
        }
        else
        {
            SteamClient = new(steamConfiguration);
        }
        _steamClientCallbackQueue = (BufferBlock<CallbackMsg>)(_steamClientCallbackQueueFieldInfo?.GetValue(SteamClient) ?? throw new Exception("SteamClient.callbackQueue 获取失败"));

        HttpClient = new();
        CallbackManager = new(SteamClient);

        _steamUser = SteamClient.GetHandler<SteamUser>() ?? throw new Exception("SteamUser获取失败");
        _steamApps = SteamClient.GetHandler<SteamApps>() ?? throw new Exception("SteamApps获取失败");
        _steamContent = SteamClient.GetHandler<SteamContent>() ?? throw new Exception("SteamContent获取失败");
        _steamCloud = SteamClient.GetHandler<SteamCloud>() ?? throw new Exception("SteamCloud获取失败");

        var steamUnifiedMessages = SteamClient.GetHandler<SteamUnifiedMessages>()!;
        _publishedFile = steamUnifiedMessages.CreateService<PublishedFile>();

        Authentication = new SteamAuthentication(this);

        CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(v =>
        {
            Disconnected?.Invoke(this, v);
        });

        PublishedFileService = new(this);
        SteamRemoteStorage = new(this);
    }

    public void Disconnect()
    {
        SteamClient.Disconnect();
        EnsureRunAllCallbacks();
    }

    public void EnsureRunAllCallbacks()
    {
        if (_steamClientCallbackQueue.TryReceiveAll(out var callbackMsgs))
        {
            foreach (var call in callbackMsgs)
            {
                Handle(CallbackManager, call);
            }
        }

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Handle")]
        static extern void Handle(CallbackManager callbackManager, CallbackMsg call);

    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (SteamClient.IsConnected)
            return;

        var lockTaken = false;
        try
        {
            await _loginLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            lockTaken = true;
            SteamClient.Connect();

            await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            if (SteamClient.IsConnected)
                return;

            if (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        await ConnectWithoutLockAsync(cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    catch (ConnectionException)
                    {
                        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                }
                throw new ConnectionException("连接失败");
            }
        }
        finally
        {
            if (lockTaken)
                _loginLock.Release();
        }
    }

    private async Task ConnectWithoutLockAsync(CancellationToken cancellationToken = default)
    {
        if (SteamClient.IsConnected)
            return;

        SteamClient.Connect();
        //await SteamClient.Connect(null, cancellationToken).ConfigureAwait(false);
        //await SteamClient.WaitConnectionCallbackAsync().ConfigureAwait(false);
        //CallbackManager.EnsureRunAllCallbacks();
        await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

        if (SteamClient.IsConnected is false)
        {
            throw new ConnectionException("连接失败");
        }
    }

    private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
    {
        while (SteamClient.IsConnected is false)
        {
            RunWaitAllCallbacks(cancellationToken);
            await Task.Yield();
        }
    }

    internal void RunWaitAllCallbacks(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallbackManager.RunWaitAllCallbacks(TimeSpan.FromSeconds(1));
    }

    public async Task EnsureConnectionLogin(CancellationToken cancellationToken = default)
    {
        EnsureRunAllCallbacks();

        if (SteamClient.IsConnected is false)
        {
            await ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        if (Authentication.Logged is false)
        {
            await Authentication.EnsureLoginAsync(cancellationToken).ConfigureAwait(false);
        }

        if (SteamClient.IsConnected is false)
            throw new ConnectionException("没有连接");

        if (Authentication.Logged is false)
            throw new ConnectionException("没有登录");
    }


    public async Task<List<SteamContentServer>> GetCdnServersAsync(uint? cellId = null, uint? max_servers = null, CancellationToken cancellationToken = default)
    {
        cellId ??= SteamClient.CellID;

        var url = new Uri(SteamClient.Configuration.WebAPIBaseAddress, $"/IContentServerDirectoryService/GetServersForSteamPipe/v1/?cell_id={cellId}{(max_servers is null ? "" : $"&max_servers={max_servers}")}");

        var jsonString = await HttpClient.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
        var servers = JsonSerializer.Deserialize<List<SteamContentServer>>(JsonNode.Parse(jsonString)?["response"]?["servers"]) ?? throw new Exception("获取失败");

        return servers;
    }

    public async ValueTask<SteamContentServer> GetRandomCdnServer(CancellationToken cancellationToken = default)
    {
        lock (_contentServersLock)
        {
            if (ContentServers.Count > 0)
                return ContentServers[Random.Shared.Next(0, ContentServers.Count)];
        }

        var r = await GetCdnServersAsync(null, null, cancellationToken).ConfigureAwait(false);
        lock (_contentServersLock)
        {
            if (ContentServers.Count == 0)
                ContentServers = r;
            if (ContentServers.Count == 0)
                throw new InvalidOperationException("没有可用的Steam CDN服务器");
            return ContentServers[Random.Shared.Next(0, ContentServers.Count)];
        }
    }

    private bool RemoveContentServer(SteamContentServer server)
    {
        lock (_contentServersLock)
        {
            return ContentServers.Remove(server);
        }
    }

    public async Task<ulong> GetAppAccessTokenAsync(uint appId, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        ulong appToken;
        if (!_appTokensCache.TryGetValue(appId, out appToken))
        {
            SteamApps.PICSTokensCallback appTokenResult = await _steamApps.PICSGetAccessTokens(appId, null).ToTask().WaitAsync(cancellationToken).ConfigureAwait(false);

            if (!appTokenResult.AppTokens.TryGetValue(appId, out appToken))
            {
                if (appTokenResult.AppTokensDenied.Contains(appId))
                {
                    throw new Exception($"权限不足  AppId:{appId}");
                }
                throw new Exception("获取失败");
            }

            if (IsCache)
            {
                foreach (var tokenKV in appTokenResult.AppTokens)
                {
                    _appTokensCache[tokenKV.Key] = tokenKV.Value;
                }
            }
        }

        return appToken;
    }

    public async Task<SteamApps.PICSProductInfoCallback.PICSProductInfo> GetProductInfoAsync(uint appId, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        var appToken = await GetAppAccessTokenAsync(appId, cancellationToken).ConfigureAwait(false);

        // 获取ProductInfo
        if (_appInfosCache.TryGetValue(appId, out var productInfo))
        {
            return productInfo;
        }

        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);
        var productInfoRequest = new SteamApps.PICSRequest(appId, appToken);
        var productInfoResult = await _steamApps.PICSGetProductInfo(productInfoRequest, null).ToTask().WaitAsync(cancellationToken).ConfigureAwait(false);

        var firstProductInfoResult = productInfoResult.Results?.FirstOrDefault();

        if (firstProductInfoResult is null)
            throw new Exception($"ProductInfo获取失败  AppId:{appId}");

        if (!firstProductInfoResult.Apps.TryGetValue(appId, out productInfo))
        {
            throw new Exception($"ProductInfo获取失败, 找不到ProductInfo  AppId:{appId}");
        }

        if (IsCache)
        {
            foreach (var item in firstProductInfoResult.Apps)
            {
                _appInfosCache[item.Key] = item.Value;
            }
        }

        return productInfo;
    }

    public async Task<ulong> GetManifestRequestCodeAsync(uint appId, uint depotId, ulong manifestId, string branch = "public", string? branchPasswordHash = null, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        ulong result;
        try
        {
            result = await _steamContent.GetManifestRequestCode(depotId, appId, manifestId, branch, branchPasswordHash).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException();
        }

        return result;
    }

    public async Task<byte[]> GetDepotKeyAsync(uint appId, uint depotId, CancellationToken cancellationToken = default)
    {
        if (_depotKeysCache.TryGetValue(depotId, out var depotKey))
        {
            return depotKey;
        }

        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        var result = await _steamApps.GetDepotDecryptionKey(depotId, appId).ToTask().WaitAsync(cancellationToken).ConfigureAwait(false);

        if (result.Result is EResult.AccessDenied)
        {
            throw new Exception($"AccessDenied  DepotId:{depotId}");
        }
        if (result.Result is not EResult.OK)
        {
            throw new Exception($"获取失败  Result:{result.Result}  DepotId:{depotId}");
        }

        if (IsCache)
        {
            _depotKeysCache[depotId] = result.DepotKey;
        }

        return result.DepotKey;
    }

    public async Task<DepotManifest> GetDepotManifestEncryptedAsync(uint depotId, ulong manifestId, ulong manifestRequestCode, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        const uint MANIFEST_VERSION = 5;

        Exception? lastException = null;
        for (int i = 0; i < 3; i++)
        {
            var server = await GetRandomCdnServer(cancellationToken).ConfigureAwait(false);
            Uri url = new(server.Url, $"/depot/{depotId}/manifest/{manifestId}/{MANIFEST_VERSION}/{manifestRequestCode}");

            Stream stream;
            nint unmanagedPtr = 0;

            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength is long len)
                {
                    if (len >= 85000)
                    {
                        unsafe
                        {
                            unmanagedPtr = (nint)NativeMemory.Alloc((nuint)len);
                            stream = new UnmanagedMemoryStream((byte*)unmanagedPtr, len, len, FileAccess.ReadWrite);
                        }
                    }
                    else
                    {
                        stream = new MemoryStream((int)len);
                    }
                }
                else
                {
                    stream = new MemoryStream();
                }
                await response.Content.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                stream.Position = 0;

                using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
                var file = zip.Entries.First();
                var bytes = new byte[file.Length];
                await file.Open().ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);

                return DepotManifest.Deserialize(bytes);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested && i < 2)
            {
                lastException = ex;
                RemoveContentServer(server);
            }
            finally
            {
                if (unmanagedPtr != 0)
                {
                    unsafe
                    {
                        NativeMemory.Free((void*)unmanagedPtr);
                    }
                }
            }
        }

        throw new HttpRequestException("Depot manifest下载失败", lastException);
    }

    public async Task<DepotManifest> GetDepotManifestAsync(uint depotId, ulong manifestId, ulong manifestRequestCode, byte[] depotKey, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        var manifestInfo = await GetDepotManifestEncryptedAsync(depotId, manifestId, manifestRequestCode, cancellationToken).ConfigureAwait(false);
        manifestInfo.DecryptFilenames(depotKey);
        return manifestInfo;
    }

    public async Task<DepotManifest> GetDepotManifestAsync(uint appId, uint depotId, ulong manifestId, string branch = "public", CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        var code = await GetManifestRequestCodeAsync(appId, depotId, manifestId, branch, null, cancellationToken).ConfigureAwait(false);
        var manifestInfo = await GetDepotManifestEncryptedAsync(depotId, manifestId, code, cancellationToken).ConfigureAwait(false);
        var key = await GetDepotKeyAsync(appId, depotId, cancellationToken).ConfigureAwait(false);
        manifestInfo.DecryptFilenames(key);
        return manifestInfo;
    }

    public async Task<DepotManifest> GetDepotManifestAsync(uint appId, uint depotId, ulong manifestId, byte[] depotKey, string branch = "public", CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        var code = await GetManifestRequestCodeAsync(appId, depotId, manifestId, branch, cancellationToken: cancellationToken).ConfigureAwait(false);
        var manifestInfo = await GetDepotManifestEncryptedAsync(depotId, manifestId, code, cancellationToken).ConfigureAwait(false);
        manifestInfo.DecryptFilenames(depotKey);
        return manifestInfo;
    }

    public Task<DepotManifest> GetWorkshopManifestAsync(uint appId, ulong hcontentFileId, CancellationToken cancellationToken = default)
    {
        return GetDepotManifestAsync(appId, appId, hcontentFileId, "public", cancellationToken);
    }

    public Task<DepotManifest> GetWorkshopManifestAsync(uint appId, ulong hcontentFileId, byte[] depotKey, CancellationToken cancellationToken = default)
    {
        return GetDepotManifestAsync(appId, appId, hcontentFileId, depotKey, "public", cancellationToken);
    }

    public async Task<byte[]> DownloadChunkDataAsync(uint depotId, DepotManifest.ChunkData chunkData, byte[] depotKey, byte[] dest, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionLogin(cancellationToken).ConfigureAwait(false);

        Exception? lastException = null;
        for (int i = 0; i < 3; i++)
        {
            var server = await GetRandomCdnServer(cancellationToken).ConfigureAwait(false);
            Uri url = new(server.Url, $"/depot/{depotId}/chunk/{Convert.ToHexString(chunkData.ChunkID!)}");

            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                var data = new byte[chunkData.CompressedLength];
                var offset = 0;

                while (await stream.ReadAsync(data.AsMemory(offset, data.Length - offset), cancellationToken).ConfigureAwait(false) is int len and > 0)
                {
                    offset += len;
                }

                if (offset != data.Length || stream.ReadByte() is int by and not -1)
                    throw new InvalidDataException("Length mismatch after downloading depot chunk!");

                DepotChunk.Process(chunkData, data, dest, depotKey);

                return dest;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested && i < 2)
            {
                lastException = ex;
                RemoveContentServer(server);
            }
        }

        throw new HttpRequestException("Depot chunk下载失败", lastException);
    }

    public Task<byte[]> DownloadChunkDataAsync(uint depotId, DepotManifest.ChunkData chunkData, byte[] depotKey, CancellationToken cancellationToken = default)
    {
        var dest = new byte[chunkData.UncompressedLength];
        return DownloadChunkDataAsync(depotId, chunkData, depotKey, dest, cancellationToken);
    }

    /// <summary>
    /// 获取创意工坊文件信息
    /// </summary>
    /// <param name="appId">AppId</param>
    /// <param name="pubFileId">Id</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<WorkshopFileDetails> GetPublishedFileAsync(uint appId, ulong pubFileId)
    {
        await EnsureConnectionLogin().ConfigureAwait(false);

        var request = new CPublishedFile_GetDetails_Request();
        request.appid = appId;
        request.publishedfileids.Add(pubFileId);

        var result = await _publishedFile.GetDetails(request).ToTask().ConfigureAwait(false);

        if (result.Result != EResult.OK)
        {
            throw new Exception($"响应失败: {result}");
        }

        var response = result.Body;
        return response.publishedfiledetails.First().ToWorkshopFileDetails();
    }

    public async Task<ICollection<WorkshopFileDetails>> GetPublishedFileAsync(uint appId, ulong[] pubFileIds)
    {
        await EnsureConnectionLogin().ConfigureAwait(false);

        var request = new CPublishedFile_GetDetails_Request();
        request.appid = appId;
        request.publishedfileids.AddRange(pubFileIds);

        var result = await _publishedFile.GetDetails(request).ToTask().ConfigureAwait(false);

        if (result.Result != EResult.OK)
        {
            throw new Exception($"响应失败: {result}");
        }

        var response = result.Body;
        return response.publishedfiledetails.Select(v => v.ToWorkshopFileDetails()).ToArray();
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        SteamClient.Disconnect();
        HttpClient.Dispose();
    }

}
