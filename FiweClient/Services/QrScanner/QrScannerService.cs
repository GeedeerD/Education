namespace FiweClient.Services.QrScanner;

#if ANDROID
// Статический делегат, который MainActivity регистрирует при старте.
// Позволяет обойти круговую зависимость FiweClient.App <-> FiweClient.Droid.
public static class QrScanBridge
{
    public static Func<Task<string?>>? ScanFunc { get; set; }
}

public class AndroidQrScannerService : IQrScannerService
{
    public bool IsAvailable => QrScanBridge.ScanFunc != null;

    public Task<string?> ScanAsync() =>
        QrScanBridge.ScanFunc?.Invoke() ?? Task.FromResult<string?>(null);
}
#else
public class DesktopQrScannerService : IQrScannerService
{
    public bool IsAvailable => false;
    public Task<string?> ScanAsync() => Task.FromResult<string?>(null);
}
#endif
