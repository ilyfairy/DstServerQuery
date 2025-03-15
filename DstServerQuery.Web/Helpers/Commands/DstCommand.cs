using DstServerQuery.Services;
using Spectre.Console;
using System.CommandLine;

namespace DstServerQuery.Web.Helpers.Commands;

public class DstCommand : Command
{
    private DstVersionService _versionService;

    public DstCommand(IServiceProvider serviceProvider) : base("dst", "饥荒服务器查询")
    {
        _versionService = serviceProvider.GetRequiredService<DstVersionService>();
        AddVersion();
    }

    public void AddVersion()
    {
        Command versionCommand = new("version", "获取程序版本信息");
        versionCommand.SetHandler(() =>
        {
            AnsiConsole.WriteLine($"当前的版本: {_versionService.Version?.ToString() ?? "获取失败"}");
        });
        AddCommand(versionCommand);
    }
}
