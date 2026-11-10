using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Crypto;
using FiweClient.Services.Session;
using QRCoder;

namespace FiweClient.ViewModels.Settings;

public partial class QrGeneratorViewModel : ObservableObject
{
    private readonly IKeyStorageService _keyStorage;
    private readonly ISessionService _session;

    [ObservableProperty] private Bitmap? _qrBitmap;
    [ObservableProperty] private string? _transferCode;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public QrGeneratorViewModel(IKeyStorageService keyStorage, ISessionService session)
    {
        _keyStorage = keyStorage;
        _session = session;
    }

    /// <summary>Кнопка «назад» в шапке — раньше с этого экрана можно было уйти только через боковое меню.</summary>
    [RelayCommand]
    private void GoBack()
        => (App.Services.GetService(typeof(ViewModels.ShellViewModel)) as ViewModels.ShellViewModel)?.OpenSettings();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var keyPair = await _keyStorage.LoadKeyPairAsync(_session.UserId!)
                ?? throw new Exception("Ключевая пара не найдена на этом устройстве");

            // Формат: publicKeyBase64|privateKeyBase64
            TransferCode = $"{keyPair.PublicKeyBase64}|{Convert.ToBase64String(keyPair.PrivateKey)}";
            QrBitmap = GenerateQrBitmap(TransferCode);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Bitmap GenerateQrBitmap(string content)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        var pngQrCode = new PngByteQRCode(qrData);
        var pngBytes = pngQrCode.GetGraphic(8); // 8px per module
        using var stream = new MemoryStream(pngBytes);
        return new Bitmap(stream);
    }
}
