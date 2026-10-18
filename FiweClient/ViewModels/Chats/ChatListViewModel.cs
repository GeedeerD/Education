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
    public string TimeLabel => LastActive.AddHours(UtcOffsetHours).ToString("HH:mm");
}

public partial class ChatListViewModel : ObservableObject
{
    private readonly IMessageApiService _messageApi;
    private readonly IAppSettingsService _appSettings;

    [ObservableProperty] private ObservableCollection<ChatPreviewViewModel> _chats = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private ChatPreviewViewModel? _selectedChat;

    public ChatListViewModel(IMessageApiService messageApi, IAppSettingsService appSettings)
    {
        _messageApi = messageApi;
        _appSettings = appSettings;
    }

    [RelayCommand]
    public async Task LoadChatsAsync()
    {
        IsBusy = true;
        try
        {
            var dtos = await _messageApi.GetAllMyChatsAsync();
            Chats = new ObservableCollection<ChatPreviewViewModel>(
                dtos.Select(d => new ChatPreviewViewModel
                {
                    ChatId = d.ChatId,
                    Name = d.Name ?? "Без имени",
                    LastMessage = d.LastMessagePreview,
                    LastActive = DateTime.SpecifyKind(d.LastActiveDateTime, DateTimeKind.Utc),
                    ContactUserId = d.RecipientId,
                    UtcOffsetHours = _appSettings.UtcOffsetHours,
                }));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Клик по чату — открываем через Shell чтобы боковая панель не пропала
    /// </summary>
    partial void OnSelectedChatChanged(ChatPreviewViewModel? value)
    {
        if (value is null) return;

        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell?.OpenChat(value.ChatId, value.Name, value.ContactUserId);
    }
}
