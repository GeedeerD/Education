using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.QrScanner;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;

namespace FiweClient.ViewModels.Auth;

public partial class KeyTransferViewModel : ObservableObject
{
    private readonly IKeyStorageService _keyStorage;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;
    private readonly ICryptoService _crypto;
    private readonly IUserApiService _userApi;
    private readonly IRealtimeService _realtime;
    private readonly IQrScannerService _qrScanner;

    [ObservableProperty] private string _transferCode = "";
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public bool IsScannerAvailable => _qrScanner.IsAvailable;

    public KeyTransferViewModel(
        IKeyStorageService keyStorage,
        ISessionService session,
        INavigationService navigation,
        ICryptoService crypto,
        IUserApiService userApi,
        IRealtimeService realtime,
        IQrScannerService qrScanner)
    {
        _keyStorage = keyStorage;
        _session = session;
        _navigation = navigation;
        _crypto = crypto;
        _userApi = userApi;
        _realtime = realtime;
        _qrScanner = qrScanner;
    }

    [RelayCommand]
    private async Task ScanQrAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await _qrScanner.ScanAsync();
            if (result != null)
                TransferCode = result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Импортирует ключевую пару из кода переноса, полученного с другого устройства.
    /// Формат кода: publicKeyBase64|privateKeyBase64
    /// </summary>
    [RelayCommand]
    private async Task ImportKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(TransferCode))
        {
            ErrorMessage = "Введите код переноса";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var parts = TransferCode.Trim().Split('|');
            if (parts.Length != 2)
                throw new FormatException();

            var publicKey = Convert.FromBase64String(parts[0]);
            var privateKey = Convert.FromBase64String(parts[1]);
            var keyPair = new KeyPair(publicKey, privateKey);

            await _keyStorage.SaveKeyPairAsync(keyPair, _session.UserId!);
            await ConnectAndNavigateToShellAsync();
        }
        catch (FormatException)
        {
            ErrorMessage = "Неверный формат кода переноса";
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

    /// <summary>
    /// Генерирует новую ключевую пару на этом устройстве.
    /// Внимание: старые зашифрованные сообщения будут недоступны.
    /// </summary>
    [RelayCommand]
    private async Task GenerateNewKeyAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var keyPair = _crypto.GenerateKeyPair();
            await _keyStorage.SaveKeyPairAsync(keyPair, _session.UserId!);
            await _userApi.SetPublicKeyAsync(keyPair.PublicKeyBase64);
            await ConnectAndNavigateToShellAsync();
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

    private async Task ConnectAndNavigateToShellAsync()
    {
        await _realtime.ConnectAsync(_session.Token!);
        var shell = App.Services.GetService(typeof(ShellViewModel)) as ShellViewModel;
        shell!.Initialize();
        _navigation.NavigateTo(shell);
    }
}
