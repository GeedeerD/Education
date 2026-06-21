using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using FiweClient.Services.QrScanner;

[assembly: Android.App.UsesPermission(Android.Manifest.Permission.Camera)]

namespace FiweClient.Droid;

[Activity(
    Label = "Fiwe",
    Theme = "@style/Theme.AppCompat.NoActionBar",
    MainLauncher = true,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges =
        ConfigChanges.Orientation |
        ConfigChanges.ScreenSize |
        ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int QrRequestCode = 2001;
    private TaskCompletionSource<string?>? _qrTcs;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Log.Debug("FIWE", "=== CustomizeAppBuilder ===");
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Log.Debug("FIWE", "=== OnCreate START ===");
        try
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Log.Error("FIWE", $"UnhandledException: {ex}");
            };

            Log.Debug("FIWE", "=== Calling base.OnCreate ===");
            base.OnCreate(savedInstanceState);
            Window?.SetSoftInputMode(SoftInput.AdjustResize);

            // Регистрируем мост для QR-сканера
            QrScanBridge.ScanFunc = StartQrScanAsync;

            Log.Debug("FIWE", "=== OnCreate DONE ===");
        }
        catch (Exception ex)
        {
            Log.Error("FIWE", $"=== OnCreate CRASH: {ex} ===");
            throw;
        }
    }

    private Task<string?> StartQrScanAsync()
    {
        _qrTcs = new TaskCompletionSource<string?>();
        var intent = new Intent(this, typeof(QrScanActivity));
        StartActivityForResult(intent, QrRequestCode);
        return _qrTcs.Task;
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == QrRequestCode)
        {
            var result = resultCode == Result.Ok ? data?.GetStringExtra("qr_result") : null;
            _qrTcs?.TrySetResult(result);
            _qrTcs = null;
        }
    }
}
