namespace FiweClient.Services.QrScanner;

public interface IQrScannerService
{
    bool IsAvailable { get; }
    Task<string?> ScanAsync();
}
