using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DstServerQuery.Web.Helpers.Commands;

public class GCCommand : Command
{
    private static TimeSpan startTime;

    [ModuleInitializer]
    public static void Initialize()
    {
        startTime = TimeSpan.FromMilliseconds(Environment.TickCount64);
    }

    public GCCommand() : base("gc", "垃圾回收器命令")
    {
        AddStatus();
        AddCollect();
    }


    public void AddStatus()
    {
        Command statusCommand = new("status", "获取垃圾回收器状态");
        statusCommand.SetHandler(() =>
        {
            var runningTime = TimeSpan.FromMilliseconds(Environment.TickCount64) - startTime;
            AnsiConsole.Write(
                new Panel(new Markup($"""
                    总运行时间: [blue]{runningTime:d\:hh\:mm\:ss}[/]
                    第0代垃圾回收次数: [blue]{GC.CollectionCount(0)}[/]
                    第1代垃圾回收次数: [blue]{GC.CollectionCount(1)}[/]
                    第2代垃圾回收次数: [blue]{GC.CollectionCount(2)}[/]
                    GC总暂停时间: [blue]{GC.GetTotalPauseDuration().TotalSeconds:0.000}s[/]
                    GC堆大小: [blue]{GC.GetTotalMemory(false) / 1024.0 / 1024.0:0.00}MB[/]
                    进程占用内存: [blue]{Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024:0.00}MB[/]
                    """))
                {
                    Header = new PanelHeader("垃圾回收器状态"),
                    BorderStyle = new Style(Color.Yellow3)
                });
        });

        AddCommand(statusCommand);
    }

    public void AddCollect()
    {
        Command collectCommand = new("collect", "执行垃圾回收");
        Option<string> g = new("--generation", "回收代系");
        g.SetDefaultValue("all");
        g.AddAlias("-g");
        g.AddCompletions("0", "1", "2", "all");
        g.AddValidator(v =>
        {
            var value = v.GetValueOrDefault()?.ToString();
            if (string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (!int.TryParse(value, out int c) || c is < 0 or > 2)
            {
                v.ErrorMessage = "参数只能是 all 或 0~2";
            }
            return;
        });
        collectCommand.AddOption(g);
        collectCommand.SetHandler(v =>
        {
            if (string.Equals(v, "all", StringComparison.OrdinalIgnoreCase))
            {
                GC.Collect();
                System.Console.WriteLine("已完成所有代系垃圾回收");
                return;
            }
            else
            {
                var c = int.Parse(v);
                GC.Collect();
                System.Console.WriteLine($"已完成第{c}代垃圾回收");
            }
        }, g);

        AddCommand(collectCommand);
    }

}
