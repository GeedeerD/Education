using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FiweClient.Markdown;
using FiweClient.ViewModels.Chats;

namespace FiweClient.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();

        // Tunnel — чтобы перехватить Enter и Ctrl+B/I/E раньше, чем их обработает сам TextBox
        MessageInputBox.AddHandler(KeyDownEvent, OnMessageInputKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ChatViewModel vm)
        {
            vm.LoadMessagesCommand.Execute(null);
        }
    }

    /// <summary>
    /// Обработчик пункта «Ответить» контекстного меню сообщения.
    /// Открывает панель предпросмотра ответа над полем ввода.
    /// </summary>
    private void OnReplyMessageClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: MessageBubbleViewModel message })
            return;

        if (DataContext is ChatViewModel vm)
            vm.ReplyToMessageCommand.Execute(message);
    }

    /// <summary>
    /// Обработчик пункта «Копировать» контекстного меню сообщения.
    /// Копирует текст именно того сообщения, по которому кликнули ПКМ.
    /// </summary>
    private async void OnCopyMessageClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: MessageBubbleViewModel message })
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        var clipboard = topLevel?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(message.HasText ? message.Text : message.PreviewText);
    }

    /// <summary>
    /// Обработчик пункта «Удалить» контекстного меню сообщения.
    /// Достаём ChatViewModel через DataContext самого ChatView (а не через
    /// поиск предка в визуальном дереве — ContextMenu открывается в отдельном
    /// попапе, и обычный $parent-биндинг команды туда не "дотягивается").
    /// </summary>
    private void OnDeleteMessageClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: MessageBubbleViewModel message })
            return;

        if (DataContext is ChatViewModel vm && vm.DeleteMessageCommand.CanExecute(message))
            vm.DeleteMessageCommand.Execute(message);
    }

    /// <summary>
    /// Обработчик кнопки «📎». Открывает системный выбор файла через
    /// Avalonia StorageProvider (работает одинаково на десктопе и Android)
    /// и добавляет выбранные изображения во ViewModel как вложения — отправка по кнопке «Отправить».
    /// </summary>
    private async void OnAttachImageClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ChatViewModel vm)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is not { } storageProvider)
            return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображения",
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        foreach (var file in files)
        {
            await using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            vm.AddAttachment(ms.ToArray(), file.Name);
        }
    }

    /// <summary>
    /// Клик по изображению в сообщении — открывает его на весь экран.
    /// Пузырь сообщения ищем по визуальным предкам: у самой картинки DataContext — MessageImageViewModel.
    /// </summary>
    private void OnBubbleImageTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: MessageImageViewModel image } control ||
            DataContext is not ChatViewModel vm)
            return;

        var bubble = control.GetVisualAncestors()
            .OfType<Control>()
            .Select(c => c.DataContext)
            .OfType<MessageBubbleViewModel>()
            .FirstOrDefault();

        if (bubble is null)
            return;

        vm.OpenImageViewer(bubble, image);
        e.Handled = true;

        // Фокус на оверлей, чтобы работали Esc и стрелки
        ImageViewerOverlay.Focus();
    }

    /// <summary>Клик по затемнённому фону просмотрщика (не по самой картинке и не по кнопкам) закрывает его.</summary>
    private void OnImageViewerBackgroundTapped(object? sender, TappedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && DataContext is ChatViewModel vm)
            vm.CloseImageViewerCommand.Execute(null);
    }

    // ── Markdown в поле ввода ─────────────────────────────────────

    private void OnMessageInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ChatViewModel vm)
            return;

        var mods = e.KeyModifiers;
        var ctrl = mods.HasFlag(KeyModifiers.Control) || mods.HasFlag(KeyModifiers.Meta);
        var shift = mods.HasFlag(KeyModifiers.Shift);

        // На Android у экранной клавиатуры нет Shift+Enter — там Enter переносит строку, а отправка кнопкой
        if (e.Key == Key.Enter && !shift && !ctrl && !OperatingSystem.IsAndroid())
        {
            e.Handled = true;
            if (vm.SendMessageCommand.CanExecute(null))
                vm.SendMessageCommand.Execute(null);
            return;
        }

        if (!ctrl)
            return;

        var action = (e.Key, shift) switch
        {
            (Key.B, false) => "bold",
            (Key.I, false) => "italic",
            (Key.X, true) => "strike",
            (Key.E, false) => "code",
            (Key.K, false) => "link",
            _ => null,
        };

        if (action is not null)
        {
            e.Handled = true;
            ApplyFormat(action);
        }
    }

    /// <summary>Кнопки панели форматирования: действие задаётся в Tag.</summary>
    private void OnFormatClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Avalonia.Controls.Button { Tag: string action })
            ApplyFormat(action);
    }

    private void ApplyFormat(string action)
    {
        var box = MessageInputBox;
        var text = box.Text ?? "";
        int start = box.SelectionStart, end = box.SelectionEnd;

        var edit = action switch
        {
            "bold" => MarkdownEditing.ToggleWrap(text, start, end, MarkdownEditing.Bold),
            "italic" => MarkdownEditing.ToggleWrap(text, start, end, MarkdownEditing.Italic),
            "strike" => MarkdownEditing.ToggleWrap(text, start, end, MarkdownEditing.Strikethrough),
            "code" => MarkdownEditing.ToggleWrap(text, start, end, MarkdownEditing.Code),
            "link" => MarkdownEditing.InsertLink(text, start, end),
            "quote" => MarkdownEditing.ToggleLinePrefix(text, start, end, "> "),
            "list" => MarkdownEditing.ToggleLinePrefix(text, start, end, "- "),
            _ => (MarkdownEdit?)null,
        };
        if (edit is not { } result)
            return;

        box.Text = result.Text;
        box.SelectionStart = result.SelectionStart;
        box.SelectionEnd = result.SelectionEnd;
        box.Focus();
    }
}
