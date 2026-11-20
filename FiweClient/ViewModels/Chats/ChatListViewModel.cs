using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Avatars;
using FiweClient.Services.Folders;
using FiweClient.Services.Navigation;
using FiweClient.Services.Session;
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

    /// <summary>Аватар собеседника: относительный URL от сервера (ChatDto.PictureUrl) или null.</summary>
    public string? PictureUrl { get; init; }

    /// <summary>Скачанный аватар; пока null — в списке показываются инициалы.</summary>
    [ObservableProperty] private Bitmap? _avatar;

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
/// Вкладка папки над списком чатов (issue #41). Первая вкладка — «Все чаты» (Model == null),
/// остальные — папки пользователя.
/// </summary>
public partial class ChatFolderViewModel : ObservableObject
{
    public const string AllChatsName = "Все чаты";

    public ChatFolderViewModel(ChatFolder? model)
    {
        Model = model;
        _name = model?.Name ?? AllChatsName;
    }

    /// <summary>Сохраняемая папка; null — вкладка «Все чаты».</summary>
    public ChatFolder? Model { get; }

    public bool IsAll => Model is null;

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isSelected;

    public bool Contains(string chatId) => IsAll || Model!.ChatIds.Contains(chatId);
}

public partial class ChatListViewModel : ObservableObject
{
    private readonly IMessageApiService _messageApi;
    private readonly IAppSettingsService _appSettings;
    private readonly IAvatarService _avatars;
    private readonly IChatFolderService _folderService;
    private readonly ISessionService _session;

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

    public string EmptyStateText
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                return "Ничего не найдено";
            if (!SelectedFolder.IsAll)
                return $"В папке «{SelectedFolder.Name}» пока нет чатов. Нажмите на чат во «Всех чатах» правой кнопкой " +
                       "(на телефоне — долгим нажатием) и выберите эту папку.";
            return "Пока нет чатов. Найдите собеседника в разделе «Контакты».";
        }
    }

    public bool ShowFindContactButton => SelectedFolder.IsAll && string.IsNullOrWhiteSpace(SearchQuery);

    public ChatListViewModel(
        IMessageApiService messageApi,
        IAppSettingsService appSettings,
        IAvatarService avatars,
        IChatFolderService folderService,
        ISessionService session)
    {
        _messageApi = messageApi;
        _appSettings = appSettings;
        _avatars = avatars;
        _folderService = folderService;
        _session = session;

        Folders = [new ChatFolderViewModel(null) { IsSelected = true }];
        _selectedFolder = Folders[0];
    }

    // ── Папки чатов (#41) ─────────────────────────────────────────

    /// <summary>Вкладки над списком: «Все чаты» и папки пользователя.</summary>
    public ObservableCollection<ChatFolderViewModel> Folders { get; }

    /// <summary>Папки пользователя без «Все чаты» — для меню «В папку» у чата.</summary>
    public IEnumerable<ChatFolderViewModel> UserFolders => Folders.Where(f => !f.IsAll);

    [ObservableProperty] private ChatFolderViewModel _selectedFolder;

    // Для какого аккаунта загружены папки (ViewModel — синглтон и переживает смену пользователя)
    private string? _foldersUserId;

    partial void OnSelectedFolderChanged(ChatFolderViewModel value)
    {
        foreach (var folder in Folders)
            folder.IsSelected = ReferenceEquals(folder, value);
        ApplyFilter();
    }

    [RelayCommand]
    private void SelectFolder(ChatFolderViewModel? folder)
    {
        if (folder is not null)
            SelectedFolder = folder;
    }

    // Строка ввода имени папки под вкладками: создание новой или переименование
    private ChatFolderViewModel? _editingFolder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderEditorWatermark))]
    private bool _isFolderEditorOpen;

    [ObservableProperty] private string _folderEditorText = "";

    public string FolderEditorWatermark => _editingFolder is null ? "Название новой папки" : "Новое название папки";

    public const int MaxFolderNameLength = 32;

    [RelayCommand]
    private void StartCreateFolder()
    {
        _editingFolder = null;
        FolderEditorText = "";
        IsFolderEditorOpen = true;
        OnPropertyChanged(nameof(FolderEditorWatermark));
    }

    public void StartRenameFolder(ChatFolderViewModel folder)
    {
        if (folder.IsAll) return;
        _editingFolder = folder;
        FolderEditorText = folder.Name;
        IsFolderEditorOpen = true;
        OnPropertyChanged(nameof(FolderEditorWatermark));
    }

    [RelayCommand]
    private void CancelFolderEditor() => IsFolderEditorOpen = false;

    [RelayCommand]
    private async Task SaveFolderEditorAsync()
    {
        var name = FolderEditorText.Trim();
        if (name.Length == 0) return;
        if (name.Length > MaxFolderNameLength) name = name[..MaxFolderNameLength];

        if (_editingFolder is null)
        {
            var folder = new ChatFolderViewModel(new ChatFolder { Name = name });
            Folders.Add(folder);
            SelectedFolder = folder;
        }
        else
        {
            _editingFolder.Name = name;
            _editingFolder.Model!.Name = name;
            OnPropertyChanged(nameof(EmptyStateText));
        }

        IsFolderEditorOpen = false;
        await SaveFoldersAsync();
    }

    public async Task DeleteFolderAsync(ChatFolderViewModel folder)
    {
        if (folder.IsAll) return;

        if (ReferenceEquals(SelectedFolder, folder))
            SelectedFolder = Folders[0];
        Folders.Remove(folder);
        await SaveFoldersAsync();
    }

    /// <summary>Добавить чат в папку или убрать из неё (меню «В папку» у чата).</summary>
    public async Task ToggleChatInFolderAsync(ChatPreviewViewModel chat, ChatFolderViewModel folder)
    {
        if (folder.IsAll) return;

        var ids = folder.Model!.ChatIds;
        if (!ids.Remove(chat.ChatId))
            ids.Add(chat.ChatId);

        if (ReferenceEquals(SelectedFolder, folder))
            ApplyFilter();
        await SaveFoldersAsync();
    }

    private async Task LoadFoldersAsync()
    {
        var userId = _session.UserId;
        if (string.IsNullOrEmpty(userId) || userId == _foldersUserId) return;
        _foldersUserId = userId;

        var saved = await _folderService.LoadAsync(userId);
        ResetFolders();
        foreach (var folder in saved)
            Folders.Add(new ChatFolderViewModel(folder));
    }

    private async Task SaveFoldersAsync()
    {
        if (string.IsNullOrEmpty(_foldersUserId)) return;
        try
        {
            await _folderService.SaveAsync(_foldersUserId, UserFolders.Select(f => f.Model!).ToList());
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось сохранить папки: {ex.Message}";
        }
    }

    private void ResetFolders()
    {
        while (Folders.Count > 1)
            Folders.RemoveAt(Folders.Count - 1);
        SelectedFolder = Folders[0];
        IsFolderEditorOpen = false;
    }

    /// <summary>Аватары догружаются фоном после списка, чтобы он не ждал картинок.</summary>
    private async Task LoadAvatarAsync(ChatPreviewViewModel chat)
    {
        var bitmap = await _avatars.GetAsync(chat.PictureUrl);
        chat.Avatar = bitmap;
        if (ActiveChat?.ChatId == chat.ChatId)
            ActiveChat.ChatAvatar = bitmap;
    }

    [RelayCommand]
    public async Task LoadChatsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await LoadFoldersAsync();
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

            foreach (var chat in _allChats.Where(c => !string.IsNullOrEmpty(c.PictureUrl)))
                _ = LoadAvatarAsync(chat);
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
        var folder = SelectedFolder;
        var filtered = _allChats
            .Where(c => folder.Contains(c.ChatId))
            .Where(c => q.Length == 0 || c.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Не теряем выделение открытого чата при фильтрации
        var activeId = ActiveChat?.ChatId;
        Chats = new ObservableCollection<ChatPreviewViewModel>(filtered);
        SelectedChat = Chats.FirstOrDefault(c => c.ChatId == activeId);

        OnPropertyChanged(nameof(ShowEmptyState));
        OnPropertyChanged(nameof(EmptyStateText));
        OnPropertyChanged(nameof(ShowFindContactButton));
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
        vm.ChatAvatar = preview?.Avatar;
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
        _foldersUserId = null;
        ResetFolders();
    }

    [RelayCommand]
    private void FindContact()
    {
        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell?.OpenContactSearch();
    }
}
