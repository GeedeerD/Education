using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Session;
using FiweClient.ViewModels.Settings;

namespace FiweClient.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IUserApiService _userApi;
    private readonly ISessionService _session;
    private readonly ShellViewModel _shell;

    [ObservableProperty] private string _publicName = "";
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public string Username => _session.Username ?? "";

    public SettingsViewModel(IUserApiService userApi, ISessionService session, ShellViewModel shell)
    {
        _userApi = userApi;
        _session = session;
        _shell = shell;
    }

    [RelayCommand]
    private async Task SavePublicNameAsync()
    {
        if (string.IsNullOrWhiteSpace(PublicName))
        {
            ErrorMessage = "Введите публичное имя";
            return;
        }

        IsBusy = true;
        SuccessMessage = null;
        ErrorMessage = null;

        try
        {
            await _userApi.SetPublicNameAsync(PublicName);
            SuccessMessage = "Публичное имя успешно обновлено";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenQrGenerator()
    {
        var vm = App.Services.GetService(typeof(QrGeneratorViewModel)) as QrGeneratorViewModel;
        _shell.CurrentContent = vm;
    }
}
