using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Avatars;
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
    private readonly IImageApiService _imageApi;
    private readonly IAvatarService _avatars;

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

    /// <summary>Форматирование Markdown для отправляемых сообщений — сохраняется сразу при переключении.</summary>
    [ObservableProperty] private bool _markdownEnabled;

    partial void OnMarkdownEnabledChanged(bool value) => _ = SaveMarkdownSettingAsync(value);

    public string Username => _session.Username ?? "";

    // ── Оформление ──
    // Тема применяется сразу и сохраняется в настройках приложения.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSystemTheme), nameof(IsLightTheme), nameof(IsDarkTheme))]
    private string _theme = "system";

    public bool IsSystemTheme { get => Theme == "system"; set { if (value) Theme = "system"; } }
    public bool IsLightTheme { get => Theme == "light"; set { if (value) Theme = "light"; } }
    public bool IsDarkTheme { get => Theme == "dark"; set { if (value) Theme = "dark"; } }

    partial void OnThemeChanged(string value)
    {
        App.ApplyTheme(value);
        _appSettings.Theme = value;
        _ = SaveSilentlyAsync();
    }

    private async Task SaveSilentlyAsync()
    {
        try { await _appSettings.SaveAsync(); }
        catch (Exception ex) { ErrorMessage = $"Не удалось сохранить настройку: {ex.Message}"; }
    }

    public SettingsViewModel(
        IUserApiService userApi,
        ISessionService session,
        ShellViewModel shell,
        IAppSettingsService appSettings,
        IImageApiService imageApi,
        IAvatarService avatars)
    {
        _userApi = userApi;
        _session = session;
        _shell = shell;
        _appSettings = appSettings;
        _imageApi = imageApi;
        _avatars = avatars;
        _utcOffsetHours = appSettings.UtcOffsetHours;
        _markdownEnabled = appSettings.MarkdownEnabled;
        _theme = appSettings.Theme; // через поле — чтобы не пересохранять настройки при открытии
    }

    // ── Аватар (#42) ──────────────────────────────────────────────

    /// <summary>Свой аватар хранится в Shell — его же показывает боковая панель.</summary>
    public ShellViewModel Shell => _shell;

    [ObservableProperty] private bool _isAvatarBusy;

    /// <summary>
    /// Новое фото профиля (байты выбранного файла — из code-behind, где открывается выбор файла):
    /// обрезаем до квадрата, загружаем на сервер и ставим аватаром.
    /// </summary>
    public async Task ChangeAvatarAsync(byte[] sourceImage)
    {
        IsAvatarBusy = true;
        SuccessMessage = null;
        ErrorMessage = null;
        try
        {
            var (png, preview) = AvatarService.PrepareForUpload(sourceImage);
            var imageId = await _imageApi.UploadImageAsync(png, "avatar.png", "image/png");
            var avatarUrl = await _userApi.SetAvatarAsync(imageId);

            if (avatarUrl != null)
                _avatars.Put(avatarUrl, preview);
            _shell.MyAvatar = preview;
            SuccessMessage = "Фото профиля обновлено";
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Старая версия сервера без Users/SetAvatar
            ErrorMessage = "Сервер ещё не поддерживает аватары — нужно задеплоить новую версию сервера.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось обновить фото: {ex.Message}";
        }
        finally
        {
            IsAvatarBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemoveAvatarAsync()
    {
        IsAvatarBusy = true;
        SuccessMessage = null;
        ErrorMessage = null;
        try
        {
            await _userApi.RemoveAvatarAsync();
            _shell.MyAvatar = null;
            SuccessMessage = "Фото профиля удалено";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось удалить фото: {ex.Message}";
        }
        finally
        {
            IsAvatarBusy = false;
        }
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
        try
        {
            await _appSettings.SaveAsync();
            SuccessMessage = "Часовой пояс сохранён";
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            SuccessMessage = null;
            ErrorMessage = $"Не удалось сохранить: {ex.Message}";
        }
    }

    private async Task SaveMarkdownSettingAsync(bool value)
    {
        _appSettings.MarkdownEnabled = value;
        try
        {
            await _appSettings.SaveAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось сохранить настройку: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenQrGenerator()
    {
        var vm = App.Services.GetService(typeof(QrGeneratorViewModel)) as QrGeneratorViewModel;
        _shell.CurrentContent = vm;
    }
}
