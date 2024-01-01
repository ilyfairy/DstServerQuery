using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text.Json.Nodes;
using Spectre.Console.Json;
using System.CommandLine;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

public class AppcalitionCommand : RootCommand
{
    public Parser? Parser { get; set; }
    public bool IsExit { get; set; }

    public AppcalitionCommand()
    {
        AddClear();
        AddExit();
        AddCommand(new GCCommand());
    }

    [MemberNotNull(nameof(Parser))]
    public void BuildParser()
    {
        Parser = new CommandLineBuilder(this)
            .UseHelp("help")
            .AddMiddleware(v =>
            {
                if (v.ParseResult.Errors.Count > 0)
                {
                    v.ExitCode = 0xffff;
                    if (v.ParseResult.Tokens.Count != 0)
                    {
                        if (v.ParseResult.UnmatchedTokens.Count > 0)
                        {
                            AnsiConsole.MarkupLine("[red]{0}[/]", Markup.Escape($"无法识别的命令或参数: {v.ParseResult.UnmatchedTokens[0]}"));
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]缺少参数[/]");
                        }
                    }

                    foreach (var target in v.ParseResult.CommandResult.Command.GetCompletions())
                    {
                        AnsiConsole.MarkupLine($"  [blue]{Markup.Escape(target.Label)}[/]   {Markup.Escape(target.Detail ?? "")}");
                    }
                }
                //v.InvocationResult = null;
            })
            .UseExceptionHandler((exception, context) =>
            {
                if (context.ExitCode == 0xffff)
                {
                    return;
                }
                AnsiConsole.Write(new Panel(Markup.Escape(exception.ToString()))
                {
                    Header = new PanelHeader("命令执行异常"),
                    BorderStyle = new Style(Color.Red),
                    Expand = true
                });
            })
            .Build();
    }

    public void AddClear()
    {
        var clearCommand = new Command("clear", "清空控制台");
        clearCommand.SetHandler(() =>
        {
            AnsiConsole.Clear();
        });

        AddCommand(clearCommand);
    }

    public void AddExit()
    {
        var exitCommand = new Command("exit", "退出");
        exitCommand.SetHandler(() =>
        {
            Console.WriteLine("bye~");
            IsExit = true;
        });

        AddCommand(exitCommand);
    }

    public void AddTest()
    {
        string testJson = """
{
    "type": "object",
    "properties": {
        "name": {
            "type": "string"
        },
        "age": {
            "type": "number"
        },
        "address": {
            "type": "object"
        },
        "phoneNumbers": {
            "type": "array",
            "items": {
                "type": "object",
                "properties": {
                    "type": {
                        "type": "string"
                    },
                    "number": {
                        "type": "string"
                    }
                },
                "additionalProperties": false
            }
        }
    },
    "additionalProperties": false
}
""";

        var ex = new Command("ex", "触发一个异常");
        ex.SetHandler(() =>
        {
            throw new Exception("异常了");
        });
        AddCommand(ex);

        var jsonCommand = new Command("json", "输出json");
        jsonCommand.SetHandler(() =>
        {
            AnsiConsole.Write(new Panel(new JsonText(testJson)) { Header = new("测试Json") });
        });
        AddCommand(jsonCommand);

        var bingimgCommand = new Command("img", "获取bing每日壁纸");
        HttpClient http = new();
        bingimgCommand.SetHandler(async () =>
        {
            try
            {
                CanvasImage? image = null;
                Stream? imageStream = null;
                await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Weather)
                        .AutoRefresh(true)
                        .StartAsync("正在获取", async v =>
                        {
                            var json = await http.GetStringAsync("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN");
                            var subPath = JsonNode.Parse(json)!["images"]![0]!["url"]!.ToString();
                            var imgUrl = new Uri(new Uri("https://www.bing.com/"), subPath);
                            imageStream = await http.GetStreamAsync(imgUrl);
                            image = new CanvasImage(imageStream);
                        });
                _ = image ?? throw new Exception("获取失败");
                AnsiConsole.Write(new Panel(image) { Header = new("Bing每日壁纸", Justify.Center) });
            }
            catch (Exception e)
            {
                AnsiConsole.Write(new Panel(e.Message)
                {
                    Header = new("图片获取失败"),
                    BorderStyle = new(Color.Red)
                });
            }
        });
        AddCommand(bingimgCommand);
    }
}
