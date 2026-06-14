using System.IdentityModel.Tokens.Jwt;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;
using FiweClient.ViewModels.Auth;

namespace FiweClient.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthApiService _authApi;
    private readonly IUserApiService _userApi;
    private readonly ISessionService _session;
    private readonly ITokenStorage _tokenStorage;
    private readonly INavigationService _navigation;
    private readonly IRealtimeService _realtime;
    private readonly ICryptoService _crypto;
    private readonly IKeyStorageService _keyStorage;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public LoginViewModel(
        IAuthApiService authApi,
        IUserApiService userApi,
        ISessionService session,
        ITokenStorage tokenStorage,
        INavigationService navigation,
        IRealtimeService realtime,
        ICryptoService crypto,
        IKeyStorageService keyStorage)
    {
        _authApi = authApi;
        _userApi = userApi;
        _session = session;
        _tokenStorage = tokenStorage;
        _navigation = navigation;
        _realtime = realtime;
        _crypto = crypto;
        _keyStorage = keyStorage;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var response = await _authApi.LoginAsync(Username, Password);
            if (response is null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            var userId = ParseUserIdFromToken(response.Token);

            // Сохраняем сессию в память
            _session.SetSession(response.Token, userId, Username);

            // Сохраняем токен на диск для авто-логина
            await _tokenStorage.SaveTokenAsync(response.Token, userId, Username);

            // Инициализируем крипто-ключи
            await InitializeCryptoAsync(userId);

            // Подключаемся к SignalR
            await _realtime.ConnectAsync(response.Token);

            // Переходим в Shell
            var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
            shell!.Initialize();
            _navigation.NavigateTo(shell);
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
    private void GoToRegister() => _navigation.NavigateTo<RegisterViewModel>();

    private async Task InitializeCryptoAsync(string userId)
    {
        KeyPair keyPair;

        if (_keyStorage.KeystoreExists(userId))
        {
            var loaded = await _keyStorage.LoadKeyPairAsync(userId);
            if (loaded is null)
            {
                keyPair = _crypto.GenerateKeyPair();
                await _keyStorage.SaveKeyPairAsync(keyPair, userId);
                await _userApi.SetPublicKeyAsync(keyPair.PublicKeyBase64);
            }
            else
            {
                keyPair = loaded;
            }
        }
        else
        {
            keyPair = _crypto.GenerateKeyPair();
            await _keyStorage.SaveKeyPairAsync(keyPair, userId);
            await _userApi.SetPublicKeyAsync(keyPair.PublicKeyBase64);
        }
    }

    private static string ParseUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims
            .First(c => c.Type == "nameid" || c.Type.EndsWith("nameidentifier"))
            .Value;
    }
}
