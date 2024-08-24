using System.CommandLine.Parsing;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;
using System.CommandLine;
using PrettyPrompt;
using Spectre.Console;
using DstServerQuery.Web.Helpers.Console;

namespace DstServerQuery.Web.Helpers.Commands;

public class CommandPromptCallbacks(Command rootCommand, ControllableConsoleSink controllableConsoleSink) : PromptCallbacks
{
    private bool isCompletionPaneOpen;
    private string lastInput = "";

    private void CheckConsoleSink()
    {
        controllableConsoleSink.Enabled = string.IsNullOrWhiteSpace(lastInput) && isCompletionPaneOpen is false;
    }

    /// <summary>
    /// 把CommandLine的补全提供给PrettyPrompt
    /// </summary>
    /// <param name="text"></param>
    /// <param name="caret"></param>
    /// <param name="spanToBeReplaced"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        var r = rootCommand.Parse(text);
        var completions = r.GetCompletions();

        IReadOnlyList<CompletionItem> result = completions.Select(v =>
        {
            var label = v.Label;
            FormattedString displayText = "";
            if (v.Kind == "Keyword")
            {
                displayText = new FormattedString(label, new ConsoleFormat(AnsiColor.Rgb(0x3d, 0x9c, 0xd6)));
            }

            var item = new CompletionItem(
                label,
                displayText,
                null,
                _ => Task.FromResult(new FormattedString(v.Detail))
                );

            return item;
        }).ToList();

        return Task.FromResult(result);
    }

    protected override Task<bool> ConfirmCompletionCommit(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
    {
        return base.ConfirmCompletionCommit(text, caret, keyPress, cancellationToken);
    }

    //当有新输入时
    protected override Task<(string Text, int Caret)> FormatInput(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
    {
        lastInput = text;
        CheckConsoleSink();

        return base.FormatInput(text, caret, keyPress, cancellationToken);
    }


    protected override Task CompletionPaneWindowStateChanged(bool isOpen)
    {
        //当补全窗口打开时，禁用日志输出
        isCompletionPaneOpen = isOpen;
        CheckConsoleSink();

        return Task.CompletedTask;
    }



    protected override Task<bool> ShouldOpenCompletionWindowAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
    {
        return base.ShouldOpenCompletionWindowAsync(text, caret, keyPress, cancellationToken);
    }

    protected override Task<TextSpan> GetSpanToReplaceByCompletionAsync(string text, int caret, CancellationToken cancellationToken)
    {
        return base.GetSpanToReplaceByCompletionAsync(text, caret, cancellationToken);
    }

    protected override IEnumerable<(KeyPressPattern Pattern, KeyPressCallbackAsync Callback)> GetKeyPressCallbacks()
    {
        return base.GetKeyPressCallbacks();
    }

    protected override Task<(IReadOnlyList<OverloadItem>, int ArgumentIndex)> GetOverloadsAsync(string text, int caret, CancellationToken cancellationToken)
    {
        return base.GetOverloadsAsync(text, caret, cancellationToken);
    }

    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
    {
        return base.HighlightCallbackAsync(text, cancellationToken);
    }

    protected override Task<KeyPress> TransformKeyPressAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
    {
        return base.TransformKeyPressAsync(text, caret, keyPress, cancellationToken);
    }
}