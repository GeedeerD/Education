using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.Settings;

namespace FiweClient.ViewModels.Chats;

public partial class MessageBubbleViewModel : ObservableObject
{
    public string Text { get; init; } = "";
    public string SenderId { get; init; } = "";
    public DateTime SentAt { get; init; }
    public bool IsMine { get; init; }
    public double UtcOffsetHours { get; init; }
    public string TimeLabel => SentAt.AddHours(UtcOffsetHours).ToString("HH:mm");
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
