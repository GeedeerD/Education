using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Navigation;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;
using FiweClient.ViewModels.Auth;
using FiweClient.ViewModels.Chats;
using FiweClient.ViewModels.Contacts;
namespace FiweClient.ViewModels;

/// <summary>
/// Главная оболочка после логина.
/// Содержит боковую панель + область контента.
/// Навигация между разделами происходит здесь — без потери боковой панели.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ITokenStorage _tokenStorage;
    private readonly IRealtimeService _realtime;

    // Текущий контент в правой области
    [ObservableProperty] private ObservableObject? _currentContent;

    // Имя пользователя в шапке боковой панели
    [ObservableProperty] private string _username = "";

    // Активная вкладка для подсветки в меню
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChatsActive), nameof(IsContactsActive), nameof(IsSettingsActive))]
    private string _activeTab = "chats";

    public bool IsChatsActive => ActiveTab == "chats";
    public bool IsContactsActive => ActiveTab == "contacts";
    public bool IsSettingsActive => ActiveTab == "settings";

    public ShellViewModel(
        INavigationService navigation,
        ISessionService session,
        ITokenStorage tokenStorage,
        IRealtimeService realtime)
    {
        _navigation = navigation;
        _session = session;
        _tokenStorage = tokenStorage;
        _realtime = realtime;

        Username = session.Username ?? "";
    }

    /// <summary>
    /// Открыть раздел чатов (левая кнопка)
    /// </summary>
    [RelayCommand]
    public void OpenChats()
    {
        ActiveTab = "chats";
        var vm = App.Services.GetService(typeof(ChatListViewModel)) as ChatListViewModel;
        CurrentContent = vm;
    }

    /// <summary>
    /// Открыть раздел контактов
    /// </summary>
    [RelayCommand]
    public void OpenContacts()
    {
        ActiveTab = "contacts";
        var vm = App.Services.GetService(typeof(ContactListViewModel)) as ContactListViewModel;
        CurrentContent = vm;
    }

    /// <summary>
    /// Открыть поиск контактов
    /// </summary>
    [RelayCommand]
    public void OpenContactSearch()
    {
        ActiveTab = "contacts";
        var vm = App.Services.GetService(typeof(AddContactViewModel)) as AddContactViewModel;
        CurrentContent = vm;
    }

    /// <summary>
    /// Открыть настройки
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        ActiveTab = "settings";
        CurrentContent = App.Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel;
    }

    /// <summary>
    /// Открыть конкретный чат. Чат показывается справа от списка чатов (двухпанельный режим),
    /// поэтому переключаемся в раздел «Чаты» и просим список открыть чат у себя.
    /// </summary>
    public void OpenChat(string chatId, string chatName, string? contactUserId = null)
    {
        ActiveTab = "chats";
        if (App.Services.GetService(typeof(ChatListViewModel)) is not ChatListViewModel list) return;
        CurrentContent = list;
        list.OpenChat(chatId, chatName, contactUserId);
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    [RelayCommand]
    private async Task LogoutAsync()
    {
        // Закрываем открытый чат, чтобы он отписался от входящих сообщений
        (App.Services.GetService(typeof(ChatListViewModel)) as ChatListViewModel)?.Reset();

        await _realtime.DisconnectAsync();
        await _tokenStorage.ClearAsync();
        _session.Clear();
        _navigation.NavigateTo<LoginViewModel>();
    }

    /// <summary>
    /// Вызывается при первом показе Shell — открываем чаты по умолчанию
    /// </summary>
    public void Initialize()
    {
        // Shell — синглтон: после выхода и входа под другим аккаунтом имя нужно обновить
        Username = _session.Username ?? "";
        OpenChats();
    }
}
