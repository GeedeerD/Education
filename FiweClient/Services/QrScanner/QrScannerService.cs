namespace FiweClient.Services.QrScanner;

#if ANDROID || IOS
// Статический делегат, который платформенный проект (MainActivity на Android,
// AppDelegate на iOS) регистрирует при старте.
// Позволяет обойти круговую зависимость FiweClient.App <-> FiweClient.Droid / FiweClient.iOS.
public static class QrScanBridge
{
    public static Func<Task<string?>>? ScanFunc { get; set; }
}

/// <summary>Сканер камерой на телефоне: сам скан делает платформенный проект через <see cref="QrScanBridge"/>.</summary>
public class MobileQrScannerService : IQrScannerService
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
