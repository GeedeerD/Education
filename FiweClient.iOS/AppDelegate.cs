using Avalonia;
using Avalonia.iOS;
using Foundation;
using FiweClient.Services.QrScanner;
using UIKit;

namespace FiweClient.iOS;

/// <summary>
/// Точка входа Avalonia на iOS — аналог MainActivity в FiweClient.Droid.
/// Всё приложение (экраны, ViewModels, сервисы) живёт в общем проекте FiweClient.App;
/// здесь только запуск и платформенные мосты (QR-сканер камерой).
/// </summary>
[Register("AppDelegate")]
#pragma warning disable CA1711 // Имя AppDelegate задано UIKit
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Console.WriteLine($"=== FiweClient iOS UnhandledException: {args.ExceptionObject} ===");

        // Мост для QR-сканера: общий код вызывает QrScanBridge.ScanFunc, не зная про UIKit
        QrScanBridge.ScanFunc = QrScanViewController.ScanAsync;

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
