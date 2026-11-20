using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FiweClient.ViewModels.Chats;

namespace FiweClient.Views;

public partial class ChatListView : UserControl
{
    // Уже этой ширины список и чат рядом не помещаются — показываем что-то одно (телефон, узкое окно)
    private const double CompactWidth = 720;

    private ChatListViewModel? _vm;

    public ChatListView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_vm is not null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;

        _vm = DataContext as ChatListViewModel;

        if (_vm is not null)
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.LoadChatsCommand.Execute(null);
        }

        UpdateLayoutMode();
    }

    // ViewModel — синглтон и живёт дольше View: отписываемся, когда View убирают с экрана
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_vm is not null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.PropertyChanged += OnViewModelPropertyChanged;
        }
        UpdateLayoutMode();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
            UpdateLayoutMode();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatListViewModel.HasActiveChat))
            UpdateLayoutMode();

        // Открыли ввод названия папки — сразу ставим туда курсор
        if (e.PropertyName == nameof(ChatListViewModel.IsFolderEditorOpen) && _vm?.IsFolderEditorOpen == true)
            Dispatcher.UIThread.Post(() =>
            {
                FolderNameBox.Focus();
                FolderNameBox.SelectAll();
            }, DispatcherPriority.Background);
    }

    // ── Папки чатов (#41) ─────────────────────────────────────────
    // Меню собираются в code-behind: ContextMenu открывается в отдельном попапе,
    // и привязки команд через $parent туда не дотягиваются (как и в ChatView).

    /// <summary>Правый клик / долгое нажатие по вкладке папки: переименовать или удалить.</summary>
    private void OnFolderContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is not Control { DataContext: ChatFolderViewModel folder } control || folder.IsAll || _vm is null)
            return;

        var rename = new MenuItem { Header = "Переименовать" };
        rename.Click += (_, _) => _vm.StartRenameFolder(folder);

        var delete = new MenuItem { Header = "Удалить папку", Classes = { "danger" } };
        delete.Click += async (_, _) => await _vm.DeleteFolderAsync(folder);

        OpenMenu(control, [rename, new Separator(), delete]);
        e.Handled = true;
    }

    /// <summary>
    /// Правый клик / долгое нажатие по чату: список папок с галочкой у тех, где чат уже есть.
    /// Клик по папке добавляет чат в неё или убирает из неё.
    /// </summary>
    private void OnChatContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is not Control { DataContext: ChatPreviewViewModel chat } control || _vm is null)
            return;

        var items = new List<Control>
        {
            new MenuItem { Header = "В папку", IsEnabled = false },
        };

        foreach (var folder in _vm.UserFolders.ToList())
        {
            var inFolder = folder.Contains(chat.ChatId);
            var item = new MenuItem
            {
                Header = folder.Name,
                Icon = inFolder
                    ? new PathIcon { Data = (Geometry)this.FindResource("IconCheck")!, Width = 14, Height = 14 }
                    : null,
            };
            item.Click += async (_, _) => await _vm.ToggleChatInFolderAsync(chat, folder);
            items.Add(item);
        }

        items.Add(new Separator());
        var create = new MenuItem { Header = "Новая папка…" };
        create.Click += (_, _) => _vm.StartCreateFolderCommand.Execute(null);
        items.Add(create);

        OpenMenu(control, items);
        e.Handled = true;
    }

    private static void OpenMenu(Control target, IEnumerable<Control> items)
    {
        var menu = new ContextMenu { ItemsSource = items };
        menu.Open(target);
    }

    private void UpdateLayoutMode()
    {
        var compact = Bounds.Width > 0 && Bounds.Width < CompactWidth;
        var chatOpen = _vm?.HasActiveChat == true;

        if (!compact)
        {
            RootGrid.ColumnDefinitions[0].Width = new GridLength(320);
            ListPane.IsVisible = true;
            ChatPane.IsVisible = true;
            Grid.SetColumnSpan(ListPane, 1);
            return;
        }

        // Узкий режим: одна колонка на всю ширину
        RootGrid.ColumnDefinitions[0].Width = new GridLength(0);
        ListPane.IsVisible = !chatOpen;
        ChatPane.IsVisible = chatOpen;
        Grid.SetColumn(ListPane, 0);
        Grid.SetColumnSpan(ListPane, 2);
    }
}
