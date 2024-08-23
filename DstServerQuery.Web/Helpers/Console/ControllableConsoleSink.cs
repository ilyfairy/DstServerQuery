using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Concurrent;
using System.Reflection;

namespace Ilyfairy.DstServerQuery.Web.Helpers.Console;

public class ControllableConsoleSink : ILogEventSink
{
    private readonly LogEventLevel? _standardErrorFromLevel;
    private readonly ITextFormatter _formatter;
    private readonly object _syncRoot = new();
    private const int DefaultWriteBufferCapacity = 256;

    public bool Enabled { get; set; } = true;
    public int HistoryLineMax { get; set; } = 1000;
    public ConcurrentQueue<LogEvent> History { get; } = new();

    static ControllableConsoleSink()
    {
        try
        {
            Assembly.Load("Serilog.Sinks.Console").GetType("Serilog.Sinks.SystemConsole.Platform.WindowsConsole")!.GetMethod("EnableVirtualTerminalProcessing")!.Invoke(null, null);
        }
        catch (Exception) { }
        //WindowsConsole.EnableVirtualTerminalProcessing();
    }

    public ControllableConsoleSink(ITextFormatter formatter, LogEventLevel? standardErrorFromLevel)
    {
        _standardErrorFromLevel = standardErrorFromLevel;
        _formatter = formatter;
    }

    public void Emit(LogEvent logEvent)
    {
        History.Enqueue(logEvent);
        if (History.Count > HistoryLineMax)
        {
            History.TryDequeue(out _);
        }

        if (!Enabled)
        {
            return;
        }
        TextWriter textWriter = SelectOutputStream(logEvent.Level);
        lock (_syncRoot)
        {
            _formatter.Format(logEvent, textWriter);
            textWriter.Flush();
        }
    }

    private TextWriter SelectOutputStream(LogEventLevel logEventLevel)
    {
        LogEventLevel? standardErrorFromLevel = _standardErrorFromLevel;
        if (!standardErrorFromLevel.HasValue)
        {
            return System.Console.Out;
        }

        if (!(logEventLevel < _standardErrorFromLevel))
        {
            return System.Console.Error;
        }

        return System.Console.Out;
    }


    public static ControllableConsoleSink Create(string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                                                 IFormatProvider? formatProvider = null,
                                                 LogEventLevel? standardErrorFromLevel = null)
    {
        ArgumentNullException.ThrowIfNull(outputTemplate);

        var OutputTemplateRendererType = Assembly.Load("Serilog.Sinks.Console").GetType("Serilog.Sinks.SystemConsole.Output.OutputTemplateRenderer")!; // .GetConstructors().First(v=>v.GetParameters().Length == 3);
        ConsoleTheme theme;
        try
        {
            theme = (ConsoleTheme)Assembly.Load("Serilog.Sinks.Console").GetType("Serilog.Sinks.SystemConsole.Themes.SystemConsoleThemes")!.GetProperty("Literate")!.GetValue(null)!;
        }
        catch (Exception)
        {
            theme = ConsoleTheme.None;
        }

        ITextFormatter formatter = (ITextFormatter)Activator.CreateInstance(OutputTemplateRendererType, [theme, outputTemplate, formatProvider])!;
        return new ControllableConsoleSink(formatter, standardErrorFromLevel);
    }
}
