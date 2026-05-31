using DstDownloaders;
using SteamDownloader;
using SteamDownloader.Helpers;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Ilyfairy.DstDownloaders;

public class ApplicationCommand : RootCommand
{
    private readonly DstDownloader dst;
    private readonly CommandArgsBinder binder;
    public int ExitCode { get; set; } = 0;
    public Parser? Parser { get; set; }
    public AppConfig AppConfig { get; set; }

    private readonly string ServerDirDefault = "DoNot Starve Together Dedicated Server";
    public readonly string ConfigPath = "config.json";

    public ApplicationCommand(DstDownloader dst) : base("饥荒联机版下载器")
    {
        this.dst = dst;
        binder = new();

        AddHandler();
        AddAllCommand();

        try
        {
            AppConfig = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new();
        }
        catch (Exception)
        {
            AppConfig = new();
        }
    }

    [MemberNotNull(nameof(Parser))]
    public void Build()
    {
        Parser = new CommandLineBuilder(this)
            .UseHelp("--help", "-h")
            .Build();
    }

    public void AddHandler()
    {
        this.SetHandler(async v =>
        {
            ExitCode = await Handle(v);
        }, binder);
    }

    private void AddAllCommand()
    {
        binder.UserName = new("--username", "Steam账号");
        binder.Password = new("--password", "Steam密码");
        binder.AccessToken = new("--access-token", "登录Token  通过账号密码登录后会自动保存到配置文件, 之后会自动从配置文件中读取");
        AddOption(binder.UserName);
        AddOption(binder.Password);
        AddOption(binder.AccessToken);


        binder.Dir = new("--dir", "所有文件保存的默认目录");
        binder.Dir.AddAlias("-D");
        AddOption(binder.Dir);


        binder.IsGetVersion = new("--dstversion", () => false, "获取饥荒服务器最新版本");
        binder.IsGetVersion.AddAlias("-V");
        AddOption(binder.IsGetVersion);


        binder.IsDownloadServer = new("--download-server", () => false, "下载饥荒服务器");
        binder.IsDownloadServer.AddAlias("-S");
        AddOption(binder.IsDownloadServer);
        binder.ServerDir = new("--server-dir", "饥荒服务器目录");
        AddOption(binder.ServerDir);


        binder.Mods = new("--mods", "需要下载的mods列表");
        binder.Mods.AddAlias("-M");
        binder.Mods.AllowMultipleArgumentsPerToken = true;
        AddOption(binder.Mods);
        binder.ModsDir = new("--mod-dir", "Mods目录, 可以使用{id}替换成ModId");
        AddOption(binder.ModsDir);
        binder.ModsUgcDir = new("--mod-ugc-dir", "UGC Mods目录, 可以使用{id}替换成ModId");
        AddOption(binder.ModsUgcDir);
    }

    public async Task<int> Handle(CommandArgs args)
    {
        if(args.Dir is { })
        {
            args.ServerDir ??= args.Dir;
            args.ModsDir ??= args.Dir;
            args.ModsUgcDir ??= args.Dir;
        }
        args.ModsDir ??= "mods";
        args.ModsUgcDir ??= args.ModsDir;
        args.ServerDir ??= ServerDirDefault;


        if(await VerifyLogin(args) is int exitCode)
        {
            return exitCode;
        }

        if(await GetCdnServers(args) is int exitCode2)
        {
            return exitCode2;
        }

        //获取饥荒版本
        if (args.IsGetVersion)
        {
            Console.WriteLine("正在获取饥荒版本...");
            try
            {
                long version = await dst.GetServerVersionAsync();
                Console.WriteLine($"饥荒最新版本: {version}");
                File.WriteAllText("version.txt", version.ToString());
            }
            catch (Exception)
            {
                Console.WriteLine("获取失败");
                return -1;
            }
        }


        //下载Server
        if (args.IsDownloadServer)
        {
            Console.WriteLine("开始下载饥荒服务器...");
            string dir = args.ServerDir!;
            DepotsSection.OS platform = DepotsSection.OS.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platform = DepotsSection.OS.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platform = DepotsSection.OS.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platform = DepotsSection.OS.MacOS;

            try
            {
                await dst.DownloadServerToDirectoryAsync(platform, dir, progress =>
                {
                    Console.WriteLine($"{(progress.IsExist ? "文件已存在" : "下载完成")}   {Math.Round(progress.CompletedFileSize / (double)progress.TotalFileSize, 4, MidpointRounding.ToZero),7:P}   {Path.Combine(dir, progress.FileData.FileName)}");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"饥荒Server-{platform}下载失败\n{e}");
                return -1;
            }

            Console.WriteLine($"饥荒Server-{platform}下载完成");
        }

        await DownloadMods(args);

        return 0;
    }

    public async Task<int?> VerifyLogin(CommandArgs args)
    {
        if (dst.Steam.SteamClient.IsConnected && dst.Steam.Authentication.Logged)
            return null;

        CancellationTokenSource cts = new();
        cts.CancelAfter(Timeout.InfiniteTimeSpan);

        args.UserName ??= AppConfig.UserName;
        args.AccessToken ??= AppConfig.AccessToken;

        Func<Task> loginAction;

        var hasUserName = !string.IsNullOrWhiteSpace(args.UserName);
        if (hasUserName && !string.IsNullOrWhiteSpace(args.AccessToken))
        {
            Console.WriteLine("正在使用AccessToken登录...");
            loginAction = () => dst.LoginAsync(args.UserName!, args.AccessToken, cts.Token).ContinueWith(task =>
            {
                Console.WriteLine("登录成功");
            });
        }
        else if (hasUserName && !string.IsNullOrWhiteSpace(args.Password))
        {
            Console.WriteLine("正在使用账号密码登录...");
            loginAction = () => dst.LoginAsync(args.UserName!, args.Password, true, cts.Token).ContinueWith(task =>
            {
                AppConfig.AccessToken = dst.AccessToken;
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(AppConfig));
                Console.WriteLine("登录成功  已保存AccessToken至config.json");
            });
        }
        else
        {
            Console.WriteLine("正在匿名登录...");
            loginAction = () => dst.LoginAsync(cts.Token).ContinueWith(task =>
            {
                Console.WriteLine("登录成功");
            });
        }

        try
        {
            await loginAction();
        }
        catch (Exception)
        {
            Console.WriteLine("登录失败, 正在重新登录...");
            try
            {
                cts = new();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                await loginAction();
            }
            catch (Exception)
            {
                Console.WriteLine("登录失败");
                return -1;
            }
        }

        return null;
    }

    public async Task<int?> GetCdnServers(CommandArgs args)
    {
        //获取cdn下载服务器
        var cdn = await dst.Steam.GetCdnServersAsync().ConfigureAwait(false);
        IReadOnlyCollection<SteamContentServer> stableCdn = await SteamHelper.TestContentServerConnectionAsync(dst.Steam.HttpClient, cdn, TimeSpan.FromSeconds(3)).ConfigureAwait(false);
        if (stableCdn.Count == 0)
            stableCdn = cdn; // 节点全超时, 可能下载速度慢
        dst.Steam.ContentServers.AddRange(stableCdn);

        return null;
    }

    public async Task<int?> DownloadMods(CommandArgs args)
    {
        if(args.Mods is null || args.Mods.Length == 0)
        {
            return null;
        }

        var defDir = args.ModsDir!;
        var ugcDir = args.ModsUgcDir!;

        Console.WriteLine();
        Console.WriteLine("开始下载Mods");
        Console.WriteLine($"Mods Dir: \t{Path.GetFullPath(defDir)}");
        Console.WriteLine($"UGC Mods Dir: \t{Path.GetFullPath(ugcDir)}");
        Console.WriteLine();

        foreach (var item in args.Mods)
        {
            Console.WriteLine($"准备下载 Mod  {item}");
        }
        Console.WriteLine();

        await Parallel.ForEachAsync(args.Mods, async (id, token) =>
        {
            var info = await dst.GetModInfoAsync(id, token).ConfigureAwait(false);
            if (!info.IsValid)
            {
                Console.WriteLine($"ModId无效: {id}");
                return;
            }

            string GetModDir()
            {
                string dir;
                if (info.IsUGC)
                    dir = ugcDir;
                else 
                    dir = defDir;

                if (dir.Contains("{id}"))
                {
                    dir = dir.Replace("{id}", id.ToString());
                }
                else
                {
                    dir = Path.Combine(dir, id.ToString());
                }
                return dir;
            }

            try
            {
                if (info.IsUGC)
                {
                    await dst.DownloadUGCModToDirectoryAsync(info.details.HContentFile, GetModDir()).ConfigureAwait(false);
                }
                else
                {
                    await dst.DownloadZipModToDirectoryAsync(info.FileUrl, GetModDir()).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"下载失败  {id}\t{info.Name}");
                throw;
            }
            Console.WriteLine($"下载成功  {id}\t{info.Name}");
        });

        return null;
    }

}
