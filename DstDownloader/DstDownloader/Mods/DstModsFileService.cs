using SteamDownloader;
using SteamDownloader.Helpers;
using SteamDownloader.WebApi.Interfaces;
using SteamKit2;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Channels;
using System.Security.Cryptography;
using System.IO.Compression;
using MoonSharp.Interpreter;
using DstDownloaders.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections;
using SteamDownloader.WebApi;
using System.Buffers;
using DstDownloaders.Converters;

namespace DstDownloaders.Mods;

public class DstModsFileService : IDisposable
{
    public DstDownloader DstSession { get; }

    private readonly ConcurrentDictionary<ulong, InternalCache> _cache = new();
    public StoresCache Cache { get; }

    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromSeconds(60);

    public string ModsRoot { get; set; }
    public string StoreFileName { get; init; } = ".store.json";
    public string ManifestFileName { get; init; } = ".manifest.bin";

    public static readonly string ModInfoLuaFileName = "modinfo.lua";
    public static readonly string ModMainLuaFileName = "modmain.lua";

    public bool IsEnableMultiLanguage { get; set; } = false;

    public byte[]? AppDepotKey { get; set; }

    public bool IsDefaultIncludeManifest { get; set; } = false;

    public JsonSerializerOptions JsonOptions { get; }

    private Script _lua;

    public Func<Uri, Uri>? FileUrlProxy => DstSession.FileUrlProxy;

    private StringCacheConverter? _stringCacheConverter;
    public StringCacheConverter? StringCacheConverter
    {
        get => _stringCacheConverter;
        set
        {
            _stringCacheConverter = value;
            if (value is null)
            {
                if (_dstModStoreJsonOptions.Converters.FirstOrDefault(v => v is StringCacheConverter) is { } converter)
                {
                    _dstModStoreJsonOptions.Converters.Remove(converter);
                }
            }
            else
            {
                _dstModStoreJsonOptions.Converters.Add(value);
            }
        }
    }
    private JsonSerializerOptions _dstModStoreJsonOptions = new();

    public DstModsFileService(DstDownloader? dstDownloader, string modsRootDirectory)
    {
        DstSession = dstDownloader ?? new();
        ModsRoot = modsRootDirectory;
        
        JsonOptions = new JsonSerializerOptions(InterfaceBase.JsonOptions);
        JsonOptions.WriteIndented = true;


        LexerGlobalOptions.IgnoreInvalid = InvalidEscapeHandling.Keep;
        LexerGlobalOptions.UnexpectedSymbolHandling = UnexpectedSymbolHandling.Ignore;
        LexerGlobalOptions.PatternMaxCalls = 10000;

        _lua = CreateScript();

        Cache = new(this);

        Directory.CreateDirectory(modsRootDirectory);
    }

    private Script CreateScript()
    {
        Script script = new(CoreModules.String | CoreModules.Math | CoreModules.Json | CoreModules.Bit32 | CoreModules.Table | CoreModules.TableIterators | CoreModules.Metatables | CoreModules.Basic, fastStackSize: 16384);
        script.Globals["locale"] = "zh";

        script.DoString("""
            ChooseTranslationTable = function(tbl)
            	return tbl[locale] or tbl[1]
            end
            """.AsMemory());

        script.Options.DebugPrint = v => { };
        script.DebuggerEnabled = false;

        return script;
    }

    public async Task InitializeAsync()
    {
        if (!DstSession.Steam.SteamClient.IsConnected || DstSession.Steam.ContentServers.Count == 0)
        {
            await DstSession.LoginAsync();
            var servers = await DstSession.Steam.GetCdnServersAsync(1);
            var stableServers = await SteamHelper.TestContentServerConnectionAsync(DstSession.Steam.HttpClient, servers, TimeSpan.FromSeconds(3));
            DstSession.Steam.ContentServers = stableServers.ToList();
        }
    }

    public async Task InitializeAsync(Func<DstDownloader, Task> callback)
    {
        await callback.Invoke(DstSession);
    }

    public async Task RunUpdateAllAsync(Action<UpdateProgressArgs>? progress, CancellationToken cancellationToken = default)
    {
        AppDepotKey ??= await DstSession.Steam.GetDepotKeyAsync(DstSession.AppId, DstSession.AppId);

        await Parallel.ForEachAsync(FastGetAllMods(cancellationToken), new ParallelOptions()
        {
            MaxDegreeOfParallelism = 5,
        }, async (item, cancellationToken) =>
        {
            try
            {
                if (item.IsValid is false)
                {
                    progress?.Invoke(new UpdateProgressArgs()
                    {
                        Type = ModsUpdateType.Failed,
                        WorkshopId = item.WorkshopId,
                        Store = null,
                        UpdateElapsed = TimeSpan.Zero,
                    });
                    return;
                }

                bool isExists = false;
                DstModStore? store = null;
                {
                    var modsPath = Path.Combine(ModsRoot, item.WorkshopId.ToString());
                    var storeFilePath = Path.Combine(modsPath, StoreFileName);
                    isExists = File.Exists(storeFilePath);
                }

                store = GetOrVerifyStore(item.WorkshopId, item.UpdatedTime);
                if (isExists && store != null)
                {
                    var isInfoUpdate = await EnsureSteamInfoUpdateAsync(store, item);
                    if (isInfoUpdate)
                    {
                        InsertCache(store);
                        SaveToFile(store);
                    }
                    progress?.Invoke(new UpdateProgressArgs()
                    {
                        Type = isInfoUpdate ? ModsUpdateType.UpdateInfo : ModsUpdateType.Valid,
                        WorkshopId = item.WorkshopId,
                        Store = store,
                        UpdateElapsed = TimeSpan.Zero,
                    });
                    return;
                }

                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        store = await DownloadAsync(item, cancellationToken);
                        progress?.Invoke(new UpdateProgressArgs()
                        {
                            Type = isExists ? ModsUpdateType.Update : ModsUpdateType.Download,
                            WorkshopId = item.WorkshopId,
                            Store = store,
                            UpdateElapsed = sw.Elapsed,
                        });
                        InsertCache(store);
                        return;
                    }
                    catch (Exception ex)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (i == 3)
                        {
                            progress?.Invoke(new UpdateProgressArgs()
                            {
                                Type = ModsUpdateType.Failed,
                                WorkshopId = item.WorkshopId,
                                Store = null,
                                UpdateElapsed = sw.Elapsed,
                                Exception = ex,
                            });

                            if (ex is ConnectionException)
                                throw;
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }

        });
    }

    public async Task RunUpdateSteamInfoAsync(CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(FastParallelGetAllMods(cancellationToken), new ParallelOptions()
        {
            MaxDegreeOfParallelism = 5,
        }, async (item, cancellationToken) =>
        {
            if (item.IsValid is false)
                return;

            bool isExists = false;
            DstModStore? store = null;
            {
                var modsPath = Path.Combine(ModsRoot, item.WorkshopId.ToString());
                var storeFilePath = Path.Combine(modsPath, StoreFileName);
                isExists = File.Exists(storeFilePath);
            }
            if (_cache.TryGetValue(item.WorkshopId, out var storeCache))
            {
                store = storeCache.Store;
            }
            else
            {
                store = GetOrVerifyStore(item.WorkshopId, item.UpdatedTime);
            }

            if (isExists && store != null)
            {
                var isInfoUpdate = await EnsureSteamInfoUpdateAsync(store, item);
                if (isInfoUpdate)
                {
                    InsertCache(store);
                    SaveToFile(store);
                }
                return;
            }
        });
    }


    public async Task RunUpdateMultiLanguageDescriptionAsync(CancellationToken cancellationToken = default)
    {
        if (IsEnableMultiLanguage)
        {
            await foreach (var item in FastParallelGetAllMultiLanguageDescription(cancellationToken))
            {
                try
                {
                    var store = GetOrVerifyStore(item.WorkshopId);
                    if (store == null)
                        continue;

                    InsertCache(store);

                    store.ExtInfo.MultiLanguage ??= new();
                    store.ExtInfo.MultiLanguage[item.Lanauage] = item.Data;

                    SaveToFile(store);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private void InsertCache(DstModStore store)
    {
        var cache = _cache.GetOrAdd(store.WorkshopId, v => new InternalCache());
        cache.Store = store;
    }

    /// <summary>
    /// 确保SteamModInfo更新
    /// </summary>
    /// <param name="store"></param>
    /// <param name="steamModInfo"></param>
    /// <returns>更新了则为true</returns>
    public async Task<bool> EnsureSteamInfoUpdateAsync(DstModStore store, SteamModInfo steamModInfo)
    {
        if (store.SteamModInfo == steamModInfo)
            return false;

        if (store.SteamModInfo is null && steamModInfo != null)
        {
            store.SteamModInfo = steamModInfo;
            return true;
        }
        if (store.SteamModInfo is null || steamModInfo is null)
            return false;

        var info = store.SteamModInfo;
        var other = steamModInfo;

        if (!info.IsValid || !other.IsValid)
            return false;

        bool isUpdate = false;

        if (info.details.Title != other.details.Title)
            isUpdate = true;

        if (info.details.FileDescription != other.details.FileDescription)
            isUpdate = true;

        if (info.details.Favorited != other.details.Favorited)
            isUpdate = true;

        if (info.details.NumCommentsPublic != other.details.NumCommentsPublic)
            isUpdate = true;

        if (info.details.Subscriptions != other.details.Subscriptions)
            isUpdate = true;

        if (info.details.Views != other.details.Views)
            isUpdate = true;

        if (info.details.LifetimeSubscriptions != other.details.LifetimeSubscriptions)
            isUpdate = true;

        if (info.details.LifetimeFavorited != other.details.LifetimeFavorited)
            isUpdate = true;

        bool isPreviewUpdate = false;
        if(info.details.PreviewUrl != other.details.PreviewUrl)
        {
            isUpdate = true;
            isPreviewUpdate = true;
        }
        if(isPreviewUpdate || store.ExtInfo.PreviewImageType is null) // 获取预览图类型
        {
            if (info.details.PreviewUrl is { } url)
            {
                try
                {
                    var imageType = await GetPreviewImageTypeAsync(url).ConfigureAwait(false);
                    if (store.ExtInfo.PreviewImageType != imageType)
                    {
                        store.ExtInfo.PreviewImageType = imageType;
                        isUpdate = true;
                    }
                }
                catch (Exception)
                {
                    store.ExtInfo.PreviewImageType = null;
                }
            }
        }

        if (other.details.Previews != null)
        {
            if (info.details.Previews?.Length != other.details.Previews.Length)
            {
                isUpdate = true;
            }
        }
        
        if (other.details.VoteData != null)
        {
            if (info.details.VoteData?.Score != other.details.VoteData.Score)
            {
                isUpdate = true;
            }
        }

        if (isUpdate)
        {
            if (info.details.FileDescription == other.details.FileDescription)
                info.details.FileDescription = other.details.FileDescription;
            if (info.details.ShortDescription == other.details.ShortDescription)
                info.details.ShortDescription = other.details.ShortDescription;
            if (info.details.PreviewUrl == other.details.PreviewUrl)
                info.details.PreviewUrl = other.details.PreviewUrl;
            if (info.details.Url == other.details.Url)
                info.details.Url = other.details.Url;
            if (info.details.FileUrl == other.details.FileUrl)
                info.details.FileUrl = other.details.FileUrl;
            if (info.details.Previews != null && other.details.Previews != null)
            {
                if (info.details.Previews.SequenceEqual(other.details.Previews))
                    info.details.Previews = other.details.Previews;
            }
            if (info.details.VoteData != null && other.details.VoteData != null)
            {
                if (info.details.VoteData == other.details.VoteData)
                    info.details.VoteData = other.details.VoteData;
            }
            if (info.details.Tags != null && other.details.Tags != null)
            {
                if (info.details.Tags.SequenceEqual(other.details.Tags))
                    info.details.Tags = other.details.Tags;
            }
            store.SteamModInfo = steamModInfo;
        }

        return isUpdate;
    }


    private async Task<string?> GetPreviewImageTypeAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await DstSession.Steam.HttpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentType?.MediaType ?? null;
        }
        catch (Exception)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await DstSession.Steam.HttpClient.SendAsync(requestMessage,  HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentType?.MediaType ?? null;
            throw;
        }
    }

    public async Task<DstModStore> DownloadAsync(SteamModInfo steamModInfo, CancellationToken cancellationToken = default)
    {
        if (steamModInfo.IsValid is false)
            throw new Exception("无效Mod");

        var dir = Path.Combine(ModsRoot, steamModInfo.WorkshopId.ToString());
        string storeFilePath = Path.Combine(dir, StoreFileName);
        string manifestFilePath = Path.Combine(dir, ManifestFileName);
        string modinfoFilePath = Path.Combine(dir, ModInfoLuaFileName);
        string modmainFilePath = Path.Combine(dir, ModMainLuaFileName);

        DstModStore store = new();

        store.WorkshopId = steamModInfo.WorkshopId;
        store.SteamModInfo = await DstSession.GetModInfoAsync(steamModInfo.WorkshopId);
        store.UpdatedTime = steamModInfo.UpdatedTime;

        Directory.CreateDirectory(dir);

        if (steamModInfo.IsUGC)
        {
            AppDepotKey ??= await DstSession.Steam.GetDepotKeyAsync(DstSession.AppId, DstSession.AppId);

            var manifest = await DstSession.Steam.GetDepotManifestAsync(DstSession.AppId, DstSession.AppId, steamModInfo.details.HContentFile, "public", cancellationToken);

            var modinfoFileData = manifest.Files!.FirstOrDefault(v => v.FileName == ModInfoLuaFileName);
            var modmainFileData = manifest.Files!.FirstOrDefault(v => v.FileName == ModMainLuaFileName);

            List<DepotManifest.FileData> fileList = new();

            if (modinfoFileData is null || modinfoFileData.TotalSize == 0)
                store.ModInfoLuaSHA1 = "0";
            else
                fileList.Add(modinfoFileData);

            if (modmainFileData is null || modmainFileData.TotalSize == 0)
                store.ModMainLuaSHA1 = "0";
            else
                fileList.Add(modmainFileData);

            bool fileValied = false;
            //下载并验证modinfo.lua和modmain.lua, 重试3次
            for (int i = 0; i < 3; i++)
            {
                await DstSession.Steam.DownloadDepotManifestToDirectoryAsync(dir, DstSession.AppId, AppDepotKey, fileList, cancellationToken);

                if (modinfoFileData != null && modinfoFileData.TotalSize != 0)
                {
                    if (!File.Exists(modinfoFilePath))
                        continue;

                    using var modinfofs = File.OpenRead(modinfoFilePath);
                    if (SHA1.HashData(modinfofs).SequenceEqual(modinfoFileData.FileHash) is false)
                    {
                        continue;
                    }
                    store.ModInfoLuaSHA1 = Convert.ToHexString(modinfoFileData.FileHash);
                }
                if (modmainFileData != null && modmainFileData.TotalSize != 0)
                {
                    if (!File.Exists(modmainFilePath))
                        continue;

                    using var modmainfs = File.OpenRead(modmainFilePath);
                    if (SHA1.HashData(modmainfs).SequenceEqual(modmainFileData.FileHash) is false)
                    {
                        continue;
                    }
                    store.ModMainLuaSHA1 = Convert.ToHexString(modmainFileData.FileHash);
                }

                fileValied = true;
                break;
            }

            if (fileValied is false)
                throw new Exception("文件损坏");

            MemoryStream ms = new();
            manifest.Serialize(ms);
            ms.Position = 0;

            store.ManifestSHA1 = Convert.ToHexString(SHA1.HashData(ms));
            ms.Position = 0;

            using FileStream fs = new(manifestFilePath, FileMode.Create);
            await ms.CopyToAsync(fs, cancellationToken);
        }
        else
        {
            if (steamModInfo.FileUrl is null)
                throw new Exception("文件URL为空");

            var response = await DstSession.Steam.HttpClient.GetAsync(FileUrlProxy?.Invoke(steamModInfo.FileUrl) ?? steamModInfo.FileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            nint unmanagedPtr = 0;
            unsafe Stream GetStream()
            {
                if (steamModInfo.FileSize < 85000)
                {
                    return new MemoryStream((int)steamModInfo.FileSize);
                }
                else
                {
                    var ptr = NativeMemory.Alloc((nuint)(steamModInfo.FileSize));
                    unmanagedPtr = (nint)ptr;
                    return new UnmanagedMemoryStream((byte*)ptr, (long)steamModInfo.FileSize, (long)steamModInfo.FileSize, FileAccess.ReadWrite);
                }
            }

            try
            {
                using Stream zipCacheStream = GetStream();

                var cts = new CancellationTokenSource();
                cts.CancelAfter(10_000 + ((int)steamModInfo.FileSize / 1024 * 5)); // 10s + 200KB/s
                await response.Content.CopyToAsync(zipCacheStream, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token);
                response.Dispose();

                using ZipArchive zip = new(zipCacheStream, ZipArchiveMode.Read);

                var modinfoData = zip.GetEntry(ModInfoLuaFileName);
                var modmainData = zip.GetEntry(ModMainLuaFileName);

                if (modinfoData is null)
                {
                    store.ModInfoLuaSHA1 = "0";
                }
                else
                {
                    store.ModInfoLuaSHA1 = Convert.ToHexString(SHA1.HashData(modinfoData.Open()));
                    using var modinfofs = File.Open(modinfoFilePath, FileMode.Create);
                    modinfoData.Open().CopyTo(modinfofs);
                }

                if (modmainData is null)
                {
                    store.ModMainLuaSHA1 = "0";
                }
                else
                {
                    store.ModMainLuaSHA1 = Convert.ToHexString(SHA1.HashData(modmainData.Open()));
                    using var modmainfs = File.Open(modmainFilePath, FileMode.Create);
                    modmainData.Open().CopyTo(modmainfs);
                }
            }
            catch (Exception)
            {
                throw;
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

        EnsureLuaInfo(store);
        File.WriteAllText(storeFilePath, JsonSerializer.Serialize(store, JsonOptions));

        return store;
    }

    public async IAsyncEnumerable<SteamModInfo> FastGetAllMods([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var firstTest = await DstSession.Steam.PublishedFileService.QueryFiles(
            numperpage: 1000,
            appid: 322330,
            return_metadata: false,
            cancellationToken: cancellationToken
            );
        uint pageItemsCount = (uint)firstTest.PublishedFileDetails!.Length;
        int allPage = (int)MathF.Ceiling(firstTest.Total / (float)pageItemsCount);
        HashSet<ulong> cache = new((int)firstTest.Total);

        for (int page = 1; page <= allPage; page++)
        {
            QueryFilesResponse? response = null;
            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    response = await DstSession.Steam.PublishedFileService.QueryFiles(
                        page: (uint)page,
                        numperpage: pageItemsCount,
                        appid: 322330,
                        return_metadata: true,
                        return_previews: true,
                        return_kv_tags: true,
                        return_vote_data: true,
                        return_tags: true,
                        cancellationToken: cancellationToken
                        );
                    break;
                }
                catch (Exception)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (i == 3)
                        throw;
                }
            }

            foreach (var details in response!.PublishedFileDetails ?? [])
            {
                SteamModInfo info = new(details);

                if (info.IsValid is false)
                    continue;

                if (cache.Contains(info.WorkshopId))
                {
                    continue;
                }
                else
                {
                    yield return info;
                }
            }
        }
    }

    public async IAsyncEnumerable<SteamModInfo> FastParallelGetAllMods([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateBounded<SteamModInfo>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });

        var firstTest = await DstSession.Steam.PublishedFileService.QueryFiles(
            numperpage: 1000,
            appid: 322330,
            return_metadata: false,
            cancellationToken: cancellationToken
            );
        uint pageItemsCount = (uint)firstTest.PublishedFileDetails!.Length;
        int allPage = (int)MathF.Ceiling(firstTest.Total / (float)pageItemsCount);
        HashSet<ulong> cache = new((int)firstTest.Total);

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken,
        };

        _ = Start();

        async Task Start()
        {
            try
            {
                await Parallel.ForEachAsync(Enumerable.Range(1, allPage), parallelOptions, async (page, cancellationToken) =>
                {
                    var response = await DstSession.Steam.PublishedFileService.QueryFiles(
                        page: (uint)page,
                        numperpage: pageItemsCount,
                        appid: 322330,
                        return_metadata: true,
                        return_previews: true,
                        return_kv_tags: true,
                        return_vote_data: true,
                        return_tags: true,
                        cancellationToken: cancellationToken
                        );

                    foreach (var details in response.PublishedFileDetails ?? [])
                    {
                        SteamModInfo info = new(details);

                        if (info.IsValid is false)
                            continue;

                        bool success = false;
                        lock (cache)
                        {
                            if (cache.Contains(info.WorkshopId))
                                continue;

                            success = true;
                            cache.Add(info.WorkshopId);
                        }
                        if (success)
                        {
                            await channel.Writer.WriteAsync(info, cancellationToken);
                        }
                    }
                });
                channel.Writer.Complete();
            }
            catch (Exception e)
            {
                channel.Writer.Complete(e);
            }
        }

        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<(ulong WorkshopId, PublishedFileServiceLanguage Lanauage, DstModStore.MutiLanguage Data)> FastParallelGetAllMultiLanguageDescription([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateBounded<(ulong WorkshopId, PublishedFileServiceLanguage Lanauage, DstModStore.MutiLanguage Data)>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });

        var firstTest = await DstSession.Steam.PublishedFileService.QueryFiles(
            numperpage: 1000,
            appid: 322330,
            return_metadata: false,
            cancellationToken: cancellationToken
            );
        uint pageItemsCount = (uint)firstTest.PublishedFileDetails!.Length;
        int allPage = (int)MathF.Ceiling(firstTest.Total / (float)pageItemsCount);

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken,
        };

        _ = Start().ConfigureAwait(false);

        async Task Start()
        {
            try
            {
                await Parallel.ForEachAsync(Enum.GetValues<PublishedFileServiceLanguage>(), parallelOptions, async (language, cancellationToken) =>
                {
                    await Parallel.ForEachAsync(Enumerable.Range(1, allPage), parallelOptions, async (page, cancellationToken) =>
                    {
                        var response = await DstSession.Steam.PublishedFileService.QueryFiles(
                            page: (uint)page,
                            numperpage: pageItemsCount,
                            appid: DstSession.AppId,
                            return_metadata: true,
                            cancellationToken: cancellationToken,
                            language: language
                            );
                        foreach (var details in response.PublishedFileDetails ?? [])
                        {
                            SteamModInfo info = new(details);

                            if (info.IsValid is false)
                                continue;

                            if (details.Language != language)
                                continue;

                            var data = new DstModStore.MutiLanguage();
                            data.Name = info.Name;
                            data.Description = info.Description;
                            await channel.Writer.WriteAsync((info.WorkshopId, details.Language, data), cancellationToken);
                        }
                    });
                });
                channel.Writer.Complete();
            }
            catch (Exception e)
            {
                channel.Writer.Complete(e);
            }
        }

        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }

    public async Task<WorkshopStorageFileDetails> FastGetModInfoLiteAsync(ulong workshopId)
    {
        return await DstSession.GetModInfoLiteAsync(workshopId);
    }

    public DstModStore? GetOrVerifyStore(ulong workshopId, DateTimeOffset? verifyUpdateTime = null)
    {
        var modsPath = Path.Combine(ModsRoot, workshopId.ToString());
        var storeFilePath = Path.Combine(modsPath, StoreFileName);
        var manifestFilePath = Path.Combine(modsPath, ManifestFileName);
        var modinfoFilePath = Path.Combine(modsPath, ModInfoLuaFileName);
        var modmainFilePath = Path.Combine(modsPath, ModMainLuaFileName);

        if (!Directory.Exists(ModsRoot) || !File.Exists(storeFilePath))
        {
            return null;
        }

        DstModStore? store;
        using FileStream metadataFile = File.Open(storeFilePath, FileMode.Open);
        try
        {
            store = JsonSerializer.Deserialize<DstModStore>(metadataFile, _dstModStoreJsonOptions)!;
        }
        catch (Exception ex)
        {
            return null;
        }

        if (store is null)
            return null;

        if (store.ModInfoLuaSHA1 is null || store.ModMainLuaSHA1 is null)
            return null;

        if (store.SteamModInfo is null)
            return null;

        if (verifyUpdateTime != null && store.UpdatedTime != verifyUpdateTime)
            return null;

        // 非UGCMod 验证
        if (store.ManifestSHA1 == null)
        {
            // 如果没有Manifest, 则它不是UGCMod
            if (store.SteamModInfo.IsUGC is true) // UGC Flags不匹配
                return null;

            if (store.ModInfoLuaSHA1 != "0")
            {
                using FileStream modinfoTempFile = File.OpenRead(modinfoFilePath);
                if (!VerifySHA1(modinfoTempFile, store.ModInfoLuaSHA1))
                    return null; // modinfo.lua损坏
            }
            if (store.ModMainLuaSHA1 != "0")
            {
                using FileStream modmainTempFile = File.OpenRead(modmainFilePath);
                if (!VerifySHA1(modmainTempFile, store.ModMainLuaSHA1))
                    return null; // modmain.lua损坏
            }

            if (store.SteamModInfo.FileSize != 0)
            {
                store.ExtInfo.Size = (long)store.SteamModInfo.FileSize;
            }
            return store;
        }
        // UGCMod验证
        else
        {
            if (!File.Exists(manifestFilePath))
                return null;

            {
                using FileStream manifestFile = File.OpenRead(manifestFilePath);
                if (!VerifySHA1(manifestFile, store.ManifestSHA1))
                    return null; //manifest损坏
            }

            var manifest = DepotManifest.LoadFromFile(manifestFilePath)!;

            if (manifest.FilenamesEncrypted)
                return null;

            DepotManifest.FileData? modinfoFileData = null;
            DepotManifest.FileData? modmainFileData = null;

            foreach (var item in manifest.Files!)
            {
                if (item.FileName == ModInfoLuaFileName)
                    modinfoFileData = item;
                else if (item.FileName == ModMainLuaFileName)
                    modmainFileData = item;
            }

            if (store.ModInfoLuaSHA1 != "0" && modinfoFileData != null)
            {
                if (!VerifySHA1(modinfoFileData.FileHash, store.ModInfoLuaSHA1))
                    return null; // manifest modinfo.lua的SHA1不一致

                using FileStream modinfoFile = File.OpenRead(modinfoFilePath);
                if (!VerifySHA1(modinfoFile, store.ModInfoLuaSHA1))
                    return null; // modinfo.lua损坏
            }
            if (store.ModMainLuaSHA1 != "0" && modmainFileData != null)
            {
                if (!VerifySHA1(modmainFileData.FileHash, store.ModMainLuaSHA1))
                    return null; // manifest modmain.lua的SHA1不一致

                using FileStream modmainFile = File.OpenRead(modmainFilePath);
                if (!VerifySHA1(modmainFile, store.ModMainLuaSHA1))
                    return null; // modmain.lua损坏
            }

            if (IsDefaultIncludeManifest)
            {
                store.Manifest = manifest;
            }
            store.ExtInfo.Size = (long)manifest.TotalUncompressedSize;
        }

        // 消除重复的多语言字符串
        if (store.ExtInfo.MultiLanguage is { } multiLanguage)
        {
            multiLanguage.TryGetValue(PublishedFileServiceLanguage.English, out var en);
            multiLanguage.TryGetValue(PublishedFileServiceLanguage.Chinese, out var zh);
            if (en != null)
            {
                if (en.Description == store.SteamModInfo.Description)
                {
                    en.Description = store.SteamModInfo.Description;
                }
            }
            if (en != null && zh != null)
            {
                if (zh.Name == en.Name)
                    zh.Name = en.Name;
                if (zh.Description == en.Description)
                    zh.Description = en.Description;
            }
        }

        return store;
    }
    private static bool VerifySHA1(Stream stream, string hexSHA1)
    {
        if(hexSHA1.Length != 40)
            return false;
        Span<byte> streamSHA1 = stackalloc byte[20];
        SHA1.HashData(stream, streamSHA1);
        Span<byte> hexSHA1Bytes = stackalloc byte[20];
        Convert.FromHexString(hexSHA1, hexSHA1Bytes, out _, out _);
        return streamSHA1.SequenceEqual(hexSHA1Bytes);
    }
    private static bool VerifySHA1(byte[] bytesSHA1, string hexSHA1)
    {
        if (hexSHA1.Length != 40)
            return false;
        if(bytesSHA1.Length != 20)
            return false;
        Span<byte> hexSHA1Bytes = stackalloc byte[20];
        Convert.FromHexString(hexSHA1, hexSHA1Bytes, out _, out _);
        return hexSHA1Bytes.SequenceEqual(bytesSHA1);
    }

    public async Task<DstModStore?> GetOrDownloadAsync(ulong workshopId, CancellationToken cancellationToken = default)
    {
        var cache = _cache.GetOrAdd(workshopId, v => new InternalCache()
        {
            UpdateDateTime = DateTimeOffset.MinValue,
            Store = null,
        });

        try
        {
            await cache.Lock.WaitAsync(cancellationToken);

            WorkshopStorageFileDetails liteInfo;

            if (DateTimeOffset.Now - cache.UpdateDateTime < CacheExpiration && cache.Store != null)
            {
                return cache.Store;
            }
            else
            {
                //在线获取
                liteInfo = await FastGetModInfoLiteAsync(workshopId);
                if (liteInfo.Result != 1)
                {
                    //如果获取失败, 则使用本地缓存
                    var localStore = GetOrVerifyStore(workshopId);
                    cache.Store = localStore;
                    cache.UpdateDateTime = DateTimeOffset.Now;
                    return localStore;
                }
            }

            var store = GetOrVerifyStore(workshopId, liteInfo.TimeUpdated);

            if (store is { })
            {
                //if (await EnsureSteamInfoUpdateAsync(store, liteInfo)) // 更新Steam信息
                //{
                //    SaveToFile(store);
                //}

                cache.Store = store;
                cache.UpdateDateTime = DateTimeOffset.Now;
                return store;
            }

            var tempInfo = await DstSession.GetModInfoAsync(workshopId).ConfigureAwait(false);
            var result = await DownloadAsync(tempInfo, cancellationToken).ConfigureAwait(false);

            cache.UpdateDateTime = DateTimeOffset.Now;
            cache.Store = result;
            return result;
        }
        finally
        {
            cache.Lock.Release();
        }
    }

    public void EnsureCache()
    {
        foreach (var store in GetAllStores())
        {
            _cache[store.WorkshopId] = new InternalCache() { UpdateDateTime = DateTimeOffset.Now, Store = store };
        }
    }

    public IEnumerable<DstModStore> GetAllStores()
    {
        var oldStringCacheMaxCount = LuaObjectJsonConverter.StringCacheMaxCount;
        var oldStringCacheMaxCharsLength = LuaObjectJsonConverter.StringCacheMaxCharsLength;
        var oldNumberCacheMaxCount = LuaObjectJsonConverter.NumberCacheMaxCount;
        LuaObjectJsonConverter.StringCacheMaxCount = 10000;
        LuaObjectJsonConverter.StringCacheMaxCharsLength = 200;
        LuaObjectJsonConverter.NumberCacheMaxCount = 10000;

        HashSet<string> tagsCache = new();

        foreach (var dir in Directory.EnumerateDirectories(ModsRoot))
        {
            if (!ulong.TryParse(Path.GetFileName(dir), out var id))
                continue;

            var info = GetOrVerifyStore(id, null);

            if (info == null)
                continue;

            // tags string intern
            foreach (ref var item in (info.SteamModInfo?.details.Tags ?? []).AsSpan())
            {
                if (tagsCache.TryGetValue(item.Tag, out var tag))
                {
                    item.Tag = tag;
                }
                else
                {
                    tagsCache.Add(item.Tag);
                }
                if (item.DisplayName is { })
                {
                    if (tagsCache.TryGetValue(item.DisplayName, out var displayName))
                    {
                        item.DisplayName = displayName;
                    }
                    else
                    {
                        tagsCache.Add(item.DisplayName);
                    }
                }
            }
            foreach (ref var tag in (info.SteamModInfo?.Tags ?? []).AsSpan())
            {
                if (tagsCache.TryGetValue(tag, out var value))
                {
                    tag = value;
                }
                else
                {
                    tagsCache.Add(tag);
                }
            }

            yield return info;
        }
        LuaObjectJsonConverter.StringCache.Clear();
        LuaObjectJsonConverter.NumberCache.Clear();
        LuaObjectJsonConverter.StringCacheMaxCount = oldStringCacheMaxCount;
        LuaObjectJsonConverter.StringCacheMaxCharsLength = oldStringCacheMaxCharsLength;
        LuaObjectJsonConverter.NumberCacheMaxCount = oldNumberCacheMaxCount;
    }

    public bool EnsureLuaInfo(DstModStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(store.SteamModInfo);

        var path = Path.Combine(ModsRoot, store.SteamModInfo.WorkshopId.ToString(), ModInfoLuaFileName);
        if (!File.Exists(path))
            return false;

        var len = new FileInfo(path).Length;
        var fileContentChars = ArrayPool<char>.Shared.Rent((int)len);
        var fileEndPosition = 0;
        ModInfoLua? info;
        try
        {
            using var sr = new StreamReader(path, System.Text.Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                fileEndPosition += sr.Read(fileContentChars.AsSpan()[fileEndPosition..]);
            }
            info = GetLuaInfo(fileContentChars.AsMemory(0, fileEndPosition), store.SteamModInfo.WorkshopId);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(fileContentChars);
        }

        if (info is null)
            return false;

        store.ModInfoLua = info;
        return true;
    }

    public ModInfoLua? GetLuaInfo(ReadOnlyMemory<char> luaCode, ulong id)
    {
        if (luaCode.Length == 0)
            return null;

        var table = new Table(_lua);
        foreach (var k in _lua.Globals.Keys)
        {
            table[k] = _lua.Globals[k];
        }
        table["folder_name"] = $"workshop-{id}";

        DynValue r;
        try
        {
            r = _lua.DoStringAndRemoveSource(luaCode, table);
            _lua.ClearByteCode();
        }
        catch (Exception e)
        {
            try
            {
                _lua = CreateScript();
                table = new Table(_lua);
                foreach (var k in _lua.Globals.Keys)
                {
                    table[k] = _lua.Globals[k];
                }
                table["folder_name"] = $"workshop-{id}";

                r = _lua.DoString(luaCode, table);
            }
            catch
            {
                return null;
            }
        }

        table.Remove("folder_name");
        foreach (var key in _lua.Globals.Keys)
        {
            table.Remove(key);
        }

        var author = table["author"];
        var name = table["name"];
        var description = table["description"];
        var version = table["version"]!;
        var api_version = table["api_version"];
        var api_version_dst = table["api_version_dst"];
        var configuration_options = table["configuration_options"] as Table;

        try
        {
            DstConfigurationOption[]? options = null;
            if (configuration_options != null)
            {
                options = ParseOptions(configuration_options);
            }

            ModInfoLua modInfo = new()
            {
                Author = (string)author,
                Name = (string)LuaConverter.CachePrimitive(name),
                Description = description switch
                {
                    string str => str,
                    Table _table => "",
                    _ => description?.ToString()
                },
                Version = version?.ToString(),
                ApiVersion = (int?)(double?)api_version,
                ApiVersionDst = (int?)(double?)api_version_dst,
                ConfigurationOptions = options,
            };

            return modInfo;
        }
        catch (Exception e)
        {
            return null;
        }

    }

    public DstConfigurationOption[] ParseOptions(Table table)
    {
        List<DstConfigurationOption> config_options = new(table.Length);
        foreach (var obj in table.Values)
        {
            var table_item = obj.Table;
            if (table_item is null)
                continue;

            DstConfigurationOption item = new();

            var name = ToString(table_item["name"]);
            var label = ToString(table_item["label"]);
            var default_value = table_item["default"];
            var options = table_item["options"] as Table;
            var hover = table_item["hover"];

            item.Name = name;
            item.Label = label;
            item.Default = LuaConverter.ToClrObject(default_value);
            item.Hover = ToString(hover);

            if (options != null)
            {
                List<DstConfigurationOptionItem> list = new(options.Length);
                foreach (var table_option in options.Values)
                {
                    if (table_option is null || table_option.IsNil())
                        continue;

                    if (table_option.Table is null)
                        continue;

                    DstConfigurationOptionItem option = new();
                    option.Description = ToString(table_option.Table!["description"])!;
                    var data = table_option.Table!["data"];
                    option.Data = LuaConverter.ToClrObject(data);
                    option.Hover = ToString(table_option.Table!["hover"]);

                    list.Add(option);
                }
                item.Options = list.ToArray();
            }

            config_options.Add(item);
        }
        return config_options.ToArray();

        static string? ToString(object obj)
        {
            return obj switch
            {
                string str => str,
                Table _table => "",
                double _double => _double.ToString(),
                bool _bool => _bool.ToString(),
                null => null,
                _ => ""
            };
        }
    }

    public void SaveToFile(DstModStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var modsPath = Path.Combine(ModsRoot, store.SteamModInfo!.WorkshopId.ToString(), StoreFileName);
        using FileStream fs = new(modsPath, FileMode.Create);
        JsonSerializer.Serialize(fs, store, JsonOptions);
    }

    public async Task<bool> IncludeManifest(DstModStore store)
    {
        if (store.SteamModInfo is null)
            return false;

        var modsPath = Path.Combine(ModsRoot, store.SteamModInfo.WorkshopId.ToString());
        var storeFilePath = Path.Combine(modsPath, StoreFileName);
        var manifestFilePath = Path.Combine(modsPath, ManifestFileName);

        if (store.ManifestSHA1 is null)
            return false;

        FileStream manifestFile = File.OpenRead(manifestFilePath);
        if ((await SHA1.HashDataAsync(manifestFile)).SequenceEqual(Convert.FromHexString(store.ManifestSHA1)) is false)
            return false; //manifest损坏

        manifestFile.Position = 0;
        using UnmanagedArray<byte> unmanagedArray = new((int)manifestFile.Length);
        await manifestFile.CopyToAsync(new MemoryStream(unmanagedArray.Array));

        var manifest = DepotManifest.Deserialize(unmanagedArray.Array);

        if (manifest.FilenamesEncrypted)
            return false;

        store.Manifest = manifest;
        return true;
    }

    public void Dispose()
    {
        _cache.Clear();
        _lua.ClearByteCode();
        DstSession.Dispose();
    }

    private class InternalCache
    {
        public DstModStore? Store { get; set; }
        public SemaphoreSlim Lock { get; } = new(1);
        public DateTimeOffset UpdateDateTime { get; set; }
    }


    public enum ModsUpdateType
    {
        /// <summary>
        /// 已存在, 并且最新
        /// </summary>
        Valid,
        /// <summary>
        /// 需要更新
        /// </summary>
        Update,
        /// <summary>
        /// 不存在, 需要下载
        /// </summary>
        Download,
        /// <summary>
        /// 下载失败
        /// </summary>
        Failed,
        /// <summary>
        /// 更新Steam信息(订阅数量, 详细信息等)
        /// </summary>
        UpdateInfo,
    }

    public class UpdateProgressArgs
    {
        public ModsUpdateType Type { get; set; }
        public ulong WorkshopId { get; set; }
        public DstModStore? Store { get; set; }
        public TimeSpan UpdateElapsed { get; set; }
        public Exception? Exception { get; set; }
    }

    public class StoresCache(DstModsFileService service) : IReadOnlyCollection<DstModStore?>
    {
        public DstModStore? this[ulong workshopId]
        {
            get => service._cache[workshopId].Store;
        }

        public int Count => service._cache.Count;

        public IEnumerable<ulong> Keys => service._cache.Select(v => v.Key);

        public IEnumerable<DstModStore?> Values => service._cache.Select(v => v.Value.Store);

        public bool ContainsKey(ulong key) => service._cache.ContainsKey(key);

        public bool TryGetValue(ulong key, out DstModStore? value)
        {
            var isOk = service._cache.TryGetValue(key, out var temp);
            value = temp?.Store;
            return isOk;
        }

        public IEnumerator<DstModStore?> GetEnumerator() => service._cache.Values.Select(v => v.Store).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}


//public class ObjectCacheJsonConverter : JsonConverter<object>
//{
//    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        if (reader.TokenType is JsonTokenType.StartObject)
//        {
//            throw new Exception("不支持Object读取");
//        }
//        return null;
//    }

//    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
//    {
//        JsonSerializer.Serialize(value);
//    }
//}