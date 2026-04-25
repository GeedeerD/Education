using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;

namespace FiweClient.ViewModels.Contacts;

// ── Поиск и добавление контакта ────────────────────────────────────

public partial class AddContactViewModel : ObservableObject
{
    private readonly IContactApiService _contactApi;

    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private ObservableCollection<UserSearchResultViewModel> _searchResults = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _statusMessage;

    public AddContactViewModel(IContactApiService contactApi)
    {
        _contactApi = contactApi;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsBusy = true;
        SearchResults.Clear();

        try
        {
            var results = await _contactApi.FindUsersAsync(SearchQuery);
            foreach (var r in results)
                SearchResults.Add(new UserSearchResultViewModel(r, this));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AddContactAsync(string userId, string userName)
    {
        try
        {
            await _contactApi.AddContactAsync(userId, userName);
            StatusMessage = $"✓ {userName} добавлен в контакты";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell?.OpenContacts();
    }
}

// ── Строка результата поиска ───────────────────────────────────────

public partial class UserSearchResultViewModel : ObservableObject
{
    private readonly AddContactViewModel _parent;

    public string UserId { get; }
    public string UserName { get; }

    public UserSearchResultViewModel(UserSearchResult dto, AddContactViewModel parent)
    {
        UserId = dto.UserId;
        UserName = dto.UserName;
        _parent = parent;
    }

    [RelayCommand]
    private Task AddAsync() => _parent.AddContactAsync(UserId, UserName);
}

// ── Список контактов ───────────────────────────────────────────────

public partial class ContactListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _contacts = [];

    [RelayCommand]
    private void OpenAddContact()
    {
        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell?.OpenContactSearch();
    }
}
