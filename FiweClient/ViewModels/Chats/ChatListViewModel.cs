using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.Settings;

namespace FiweClient.ViewModels.Chats;

public partial class MessageBubbleViewModel : ObservableObject
{
    public string MessageId { get; init; } = "";
    public string Text { get; init; } = "";
    public string SenderId { get; init; } = "";
    public DateTime SentAt { get; init; }
    public bool IsMine { get; init; }
    public double UtcOffsetHours { get; init; }
    public string TimeLabel => SentAt.AddHours(UtcOffsetHours).ToString("HH:mm");

    /// <summary>Вложенные изображения (пусто — обычное текстовое сообщение). Text у такого сообщения — подпись.</summary>
    public IReadOnlyList<MessageImageViewModel> Images { get; init; } = [];
    public bool IsImage => Images.Count > 0;
    public bool IsSingleImage => Images.Count == 1;
    public bool IsAlbum => Images.Count > 1;
    public MessageImageViewModel? FirstImage => Images.Count > 0 ? Images[0] : null;
    public bool HasText => !string.IsNullOrEmpty(Text);

    /// <summary>Отправлено «без форматирования» — текст показывается как есть, без разбора Markdown.</summary>
    public bool IsPlainText { get; init; }
    public bool ShowMarkdown => HasText && !IsPlainText;
    public bool ShowPlainText => HasText && IsPlainText;

    /// <summary>Короткое представление сообщения для панели ответа и копирования.</summary>
    public string PreviewText => !IsImage
        ? Text
        : HasText ? $"📷 {Text}" : Images.Count == 1 ? "📷 Фото" : $"📷 Фото ({Images.Count})";

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isSelectionMode;
}

/// <summary>Одно изображение внутри сообщения. Bitmap догружается с сервера асинхронно.</summary>
public partial class MessageImageViewModel : ObservableObject
{
    public string ImageId { get; init; } = "";

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private Bitmap? _bitmap;

    public bool HasBitmap => Bitmap is not null;
    partial void OnBitmapChanged(Bitmap? value) => OnPropertyChanged(nameof(HasBitmap));
}

/// <summary>Изображение, выбранное для отправки, но ещё не отправленное (предпросмотр над полем ввода).</summary>
public class PendingAttachmentViewModel
{
    public required byte[] Bytes { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Bitmap Thumbnail { get; init; }
}

public partial class ChatPreviewViewModel : ObservableObject
{
    public string ChatId { get; init; } = "";
    public string Name { get; init; } = "";
    public string? LastMessage { get; init; }
    public DateTime LastActive { get; init; }
    public string? ContactUserId { get; init; }
    public double UtcOffsetHours { get; init; }

    /// <summary>
    /// Ссылка на картинку чата от сервера (ChatDto.PictureUrl). Пока не загружается —
    /// задел под issue #42 «User Avatars»: загрузить Bitmap и отдать его в AvatarView.Source.
    /// </summary>
    public string? PictureUrl { get; init; }

    /// <summary>Сегодня — время, на этой неделе — день недели, раньше — дата.</summary>
    public string TimeLabel
    {
        get
        {
            var local = LastActive.AddHours(UtcOffsetHours);
            var today = DateTime.UtcNow.AddHours(UtcOffsetHours).Date;
            if (local.Date == today) return local.ToString("HH:mm");
            if (local.Date > today.AddDays(-7)) return local.ToString("ddd", Ru);
            return local.ToString(local.Year == today.Year ? "d MMM" : "dd.MM.yy", Ru);
        }
    }

    private static readonly System.Globalization.CultureInfo Ru = new("ru-RU");
}

/// <summary>
/// Папка чатов. Задел под issue #41 «Add opportunity groupping chats»:
/// сейчас есть только «Все чаты», создание своих папок ещё не реализовано.
/// </summary>
public class ChatFolderViewModel
{
    public required string Name { get; init; }
    public bool IsAll { get; init; }
}

public partial class ChatListViewModel : ObservableObject
{
    private readonly IMessageApiService _messageApi;
    private readonly IAppSettingsService _appSettings;

    // Полный список с сервера; Chats — отфильтрованный поиском
    private List<ChatPreviewViewModel> _allChats = [];

    [ObservableProperty] private ObservableCollection<ChatPreviewViewModel> _chats = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool _isBusy;

    [ObservableProperty] private ChatPreviewViewModel? _selectedChat;
    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private string? _errorMessage;

    /// <summary>Чат, открытый справа от списка (null — показывается заглушка «Выберите чат»).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveChat))]
    private ChatViewModel? _activeChat;

    public bool HasActiveChat => ActiveChat is not null;

    public bool ShowEmptyState => !IsBusy && Chats.Count == 0;
    public string EmptyStateText => string.IsNullOrWhiteSpace(SearchQuery)
        ? "Пока нет чатов. Найдите собеседника в разделе «Контакты»."
        : "Ничего не найдено";

    // ── Папки (заготовка под #41) ──
    public IReadOnlyList<ChatFolderViewModel> Folders { get; } =
        [new ChatFolderViewModel { Name = "Все чаты", IsAll = true }];

    [ObservableProperty] private ChatFolderViewModel? _selectedFolder;

    public ChatListViewModel(IMessageApiService messageApi, IAppSettingsService appSettings)
    {
        _messageApi = messageApi;
        _appSettings = appSettings;
        _selectedFolder = Folders[0];
    }

    [RelayCommand]
    public async Task LoadChatsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var dtos = await _messageApi.GetAllMyChatsAsync();
            _allChats = dtos
                .Select(d => new ChatPreviewViewModel
                {
                    ChatId = d.ChatId,
                    Name = d.Name ?? "Без имени",
                    LastMessage = d.LastMessagePreview,
                    LastActive = DateTime.SpecifyKind(d.LastActiveDateTime, DateTimeKind.Utc),
                    ContactUserId = d.RecipientId,
                    PictureUrl = d.PictureUrl,
                    UtcOffsetHours = _appSettings.UtcOffsetHours,
                })
                .OrderByDescending(c => c.LastActive)
                .ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить чаты: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = SearchQuery.Trim();
        var filtered = string.IsNullOrEmpty(q)
            ? _allChats
            : _allChats.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        // Не теряем выделение открытого чата при фильтрации
        var activeId = ActiveChat?.ChatId;
        Chats = new ObservableCollection<ChatPreviewViewModel>(filtered);
        SelectedChat = Chats.FirstOrDefault(c => c.ChatId == activeId);

        OnPropertyChanged(nameof(ShowEmptyState));
        OnPropertyChanged(nameof(EmptyStateText));
    }

    /// <summary>Клик по чату в списке — открываем его справа.</summary>
    partial void OnSelectedChatChanged(ChatPreviewViewModel? value)
    {
        if (value is null || value.ChatId == ActiveChat?.ChatId) return;
        OpenChat(value.ChatId, value.Name, value.ContactUserId);
    }

    /// <summary>Открыть чат в правой панели (из списка или из другого раздела через Shell).</summary>
    public void OpenChat(string chatId, string chatName, string? contactUserId = null)
    {
        if (ActiveChat?.ChatId == chatId) return;

        if (App.Services.GetService(typeof(ChatViewModel)) is not ChatViewModel vm) return;

        ActiveChat?.Deactivate();
        vm.Initialize(chatId, chatName, contactUserId);
        vm.CloseRequested += CloseChat;
        ActiveChat = vm;

        var preview = Chats.FirstOrDefault(c => c.ChatId == chatId);
        if (!ReferenceEquals(SelectedChat, preview))
            SelectedChat = preview;
    }

    [RelayCommand]
    public void CloseChat()
    {
        ActiveChat?.Deactivate();
        ActiveChat = null;
        SelectedChat = null;
    }

    /// <summary>Сброс при выходе из аккаунта: ViewModel — синглтон и переживает смену пользователя.</summary>
    public void Reset()
    {
        CloseChat();
        _allChats = [];
        SearchQuery = "";
        Chats = [];
    }

    [RelayCommand]
    private void FindContact()
    {
        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell?.OpenContactSearch();
    }
}
