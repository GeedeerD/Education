using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Markdown.Avalonia;
using MarkdownEngine = Markdown.Avalonia.Markdown;

namespace FiweClient.Markdown;

/// <summary>
/// MarkdownScrollViewer для пузырей сообщений: один общий движок на все сообщения
/// и безопасное открытие ссылок через системный Launcher (работает и на десктопе, и на Android).
/// </summary>
public class ChatMarkdownViewer : MarkdownScrollViewer
{
    private static MarkdownEngine? s_engine;

    public ChatMarkdownViewer()
    {
        // Движок создаётся лениво в UI-потоке и переиспользуется: разбор Markdown у него без состояния,
        // а создавать парсеры заново для каждого пузыря дорого.
        Engine = s_engine ??= new MarkdownEngine { HyperlinkCommand = new AsyncRelayCommand<string?>(OpenLinkAsync) };
    }

    protected override Type StyleKeyOverride => typeof(MarkdownScrollViewer);

    /// <summary>
    /// Открывает ссылку из сообщения в системном браузере. Разрешены только http(s) и mailto:
    /// текст пишет собеседник, и открывать file:// или произвольные схемы нельзя.
    /// </summary>
    private static async Task OpenLinkAsync(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https" or "mailto"))
            return;

        var topLevel = Avalonia.Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null,
        };

        if (topLevel?.Launcher is { } launcher)
            await launcher.LaunchUriAsync(uri);
    }
}
