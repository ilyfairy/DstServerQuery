using System.CommandLine.Parsing;
using DstDownloaders;

namespace Ilyfairy.DstDownloaders;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            args = ["--help"];
        }

        using DstDownloader dst = new();
        ApplicationCommand command = new(dst);
        command.Build();

        await command.Parser.InvokeAsync(args);

        return command.ExitCode;
    }
}
