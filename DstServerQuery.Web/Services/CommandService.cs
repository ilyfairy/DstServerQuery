using DstServerQuery.Web.Helpers.Commands;
using DstServerQuery.Web.Helpers.Console;
using PrettyPrompt;
using PrettyPrompt.Highlighting;
using System.CommandLine.Parsing;

namespace DstServerQuery.Web.Services;

public class CommandService(
    IHost host,
    ControllableConsoleSink controllableConsoleSink
    )
{
    private readonly CancellationTokenSource cts = new();
    public Prompt? Prompt { get; private set; }
    public AppcalitionCommand Command { get; private set; } = new();
    public CommandPromptCallbacks? CommandPromptCallbacks { get; set; }

    public async Task RunCommandLoopAsync()
    {
        Command.BuildParser();
        CommandPromptCallbacks = new(Command, controllableConsoleSink);

        Prompt = new(Path.Join(AppContext.BaseDirectory, "history_command.txt"), CommandPromptCallbacks, null, new PromptConfiguration(
            completionBoxBorderFormat: new ConsoleFormat(AnsiColor.Rgb(0x87, 0x6C, 0xB3)),
            selectedCompletionItemBackground: AnsiColor.Rgb(0x30, 0x30, 0x30)
            )
        {
            Prompt = new FormattedString(">>> ", new FormatSpan(0, 3, AnsiColor.BrightCyan)),
        });

        while (true)
        {
            var result = await Prompt!.ReadLineAsync();

            await Command.Parser!.InvokeAsync(result.Text);
            if (Command.IsExit || cts.Token.IsCancellationRequested)
            {
                controllableConsoleSink.Enabled = true;
                await host.StopAsync();
                break;
            }
        }
    }
}
