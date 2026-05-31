using System.CommandLine;
using System.CommandLine.Binding;

namespace Ilyfairy.DstDownloaders;

public class CommandArgs
{
    public bool IsGetVersion { get; set; }

    public bool IsDownloadServer { get; set; }

    /// <summary>
    /// ServerDir和ModDir和ModUgcDir的默认目录
    /// </summary>
    public string? Dir { get; set; }

    public string? ServerDir { get; set; }

    public string? ModsDir { get; set; }

    public string? ModsUgcDir { get; set; }

    public ulong[]? Mods { get; set; }

    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? AccessToken { get; set; }

}

public class CommandArgsBinder : BinderBase<CommandArgs>
{
    public Option<bool> IsGetVersion { get; set; } = null!;
    public Option<bool> IsDownloadServer { get; set; } = null!;
    public Option<string> Dir { get; set; } = null!;
    public Option<string> ServerDir { get; set; } = null!;
    public Option<string> ModsDir { get; set; } = null!;
    public Option<string> ModsUgcDir { get; set; } = null!;
    public Option<ulong[]?> Mods { get; set; } = null!;
    public Option<string> UserName { get; set; } = null!;
    public Option<string> Password { get; set; } = null!;
    public Option<string> AccessToken { get; set; } = null!;

    protected override CommandArgs GetBoundValue(BindingContext bindingContext)
    {
        CommandArgs args = new();

        args.Dir = bindingContext.ParseResult.GetValueForOption(Dir);

        args.IsGetVersion = bindingContext.ParseResult.GetValueForOption(IsGetVersion);

        args.IsDownloadServer = bindingContext.ParseResult.GetValueForOption(IsDownloadServer);
        args.ServerDir = bindingContext.ParseResult.GetValueForOption(ServerDir);

        args.ModsDir = bindingContext.ParseResult.GetValueForOption(ModsDir);
        args.ModsUgcDir = bindingContext.ParseResult.GetValueForOption(ModsUgcDir);
        args.Mods = bindingContext.ParseResult.GetValueForOption(Mods);

        args.UserName = bindingContext.ParseResult.GetValueForOption(UserName);
        args.Password = bindingContext.ParseResult.GetValueForOption(Password);
        args.AccessToken = bindingContext.ParseResult.GetValueForOption(AccessToken);

        return args;
    }
}