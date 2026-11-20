using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
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
