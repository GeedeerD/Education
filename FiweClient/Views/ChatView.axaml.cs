using Avalonia.Controls;
using Avalonia.Interactivity;
using FiweClient.ViewModels.Chats;

namespace FiweClient.Views;

public partial class ChatView : UserControl
{
    public ChatView() => InitializeComponent();

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
            await clipboard.SetTextAsync(message.Text);
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
}