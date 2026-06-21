using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Session;
using FiweClient.Services.Settings;
using FiweClient.ViewModels.Settings;

namespace FiweClient.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IUserApiService _userApi;
    private readonly ISessionService _session;
    private readonly ShellViewModel _shell;
    private readonly IAppSettingsService _appSettings;

    [ObservableProperty] private string _publicName = "";
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OffsetLabel))]
    private double _utcOffsetHours;

    public string OffsetLabel
    {
        get
        {
            var sign = UtcOffsetHours >= 0 ? "+" : "";
            var h = (int)Math.Truncate(UtcOffsetHours);
            var m = (int)Math.Round(Math.Abs(UtcOffsetHours - h) * 60);
            return m == 0 ? $"UTC{sign}{h}" : $"UTC{sign}{h}:{m:00}";
        }
    }

    public string Username => _session.Username ?? "";

    public SettingsViewModel(IUserApiService userApi, ISessionService session, ShellViewModel shell, IAppSettingsService appSettings)
    {
        _userApi = userApi;
        _session = session;
        _shell = shell;
        _appSettings = appSettings;
        _utcOffsetHours = appSettings.UtcOffsetHours;
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
    private async Task SaveTimezoneAsync()
    {
        _appSettings.UtcOffsetHours = UtcOffsetHours;
        await _appSettings.SaveAsync();
        SuccessMessage = "Часовой пояс сохранён";
        ErrorMessage = null;
    }

    [RelayCommand]
    private void OpenQrGenerator()
    {
        var vm = App.Services.GetService(typeof(QrGeneratorViewModel)) as QrGeneratorViewModel;
        _shell.CurrentContent = vm;
    }
}
