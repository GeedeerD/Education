using Android.Content.PM;
using Android.Util;
using Android.Views;
using Avalonia;
using Avalonia.Android;

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
            Log.Debug("FIWE", "=== OnCreate DONE ===");
        }
        catch (Exception ex)
        {
            Log.Error("FIWE", $"=== OnCreate CRASH: {ex} ===");
            throw;
        }
    }
}
