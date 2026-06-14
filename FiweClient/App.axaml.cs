using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;
using FiweClient.Services.Settings;
using FiweClient.ViewModels;
using FiweClient.ViewModels.Auth;
using FiweClient.ViewModels.Chats;
using FiweClient.ViewModels.Contacts;
using FiweClient.ViewModels.Settings;
using FiweClient.Views;
using Microsoft.Extensions.DependencyInjection;

using Application = Avalonia.Application;

namespace FiweClient;

public class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Console.WriteLine("=== FiweClient: App starting ===");

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            Console.WriteLine("=== FiweClient: Services ready ===");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Console.WriteLine("=== FiweClient: Desktop ===");
                var mainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>()
                };

                var nav = Services.GetRequiredService<INavigationService>();
                nav.SetHost(mainWindow);

                desktop.MainWindow = mainWindow;

                _ = TryAutoLoginAsync();
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                Console.WriteLine("=== FiweClient: SingleView ===");
                var mainView = new MainView
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>()
                };
                var nav = Services.GetRequiredService<INavigationService>();
                nav.SetHost(mainView);
                singleView.MainView = mainView;
                _ = TryAutoLoginAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== FiweClient CRASH: {ex} ===");
            throw;
        }
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// При старте проверяем сохранённый токен.
    /// Если валидный — сразу в Shell, иначе — на Login.
    /// </summary>
    private static async Task TryAutoLoginAsync()
    {
        var tokenStorage = Services.GetRequiredService<ITokenStorage>();
        var session = Services.GetRequiredService<ISessionService>();
        var nav = Services.GetRequiredService<INavigationService>();
        var realtime = Services.GetRequiredService<IRealtimeService>();
        var keyStorage = Services.GetRequiredService<IKeyStorageService>();
        var appSettings = Services.GetRequiredService<IAppSettingsService>();

        await appSettings.LoadAsync();
        var saved = await tokenStorage.LoadTokenAsync();

        if (saved is not null && session.IsTokenValid(saved.Token))
        {
            session.SetSession(saved.Token, saved.UserId, saved.Username);

            // Если ключей нет на устройстве — предлагаем перенос (без реалтайма)
            if (!keyStorage.KeystoreExists(saved.UserId))
            {
                var keyTransferVm = Services.GetRequiredService<KeyTransferViewModel>();
                nav.NavigateTo(keyTransferVm);
                return;
            }

            await realtime.ConnectAsync(saved.Token);

            var shell = Services.GetRequiredService<ShellViewModel>();
            shell.Initialize();
            nav.NavigateTo(shell);
        }
        else
        {
            await tokenStorage.ClearAsync();
            nav.NavigateTo<LoginViewModel>();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // ── HTTP ──────────────────────────────────────
        services.AddHttpClient("FiweApi", client =>
        {
            client.BaseAddress = new Uri("https://fiwe-api-dgabh9axgnhagqhg.italynorth-01.azurewebsites.net/");
        });

        // ── Крипто ───────────────────────────────────
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IKeyStorageService, KeyStorageService>();
        services.AddSingleton<ISharedSecretCache, SharedSecretCache>();

        // ── Сессия ────────────────────────────────────
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ITokenStorage, TokenStorage>();

        // ── API ───────────────────────────────────────
        services.AddTransient<IAuthApiService, AuthApiService>();
        services.AddTransient<IMessageApiService, MessageApiService>();
        services.AddTransient<IContactApiService, ContactApiService>();
        services.AddTransient<IUserApiService, UserApiService>();

        // ── SignalR ───────────────────────────────────
        services.AddSingleton<IRealtimeService, SignalRService>();

        // ── Настройки приложения ──────────────────────
        services.AddSingleton<IAppSettingsService, AppSettingsService>();

        // ── Навигация ─────────────────────────────────
        services.AddSingleton<INavigationService, NavigationService>();

        // ── ViewModels ────────────────────────────────
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<KeyTransferViewModel>();
        services.AddSingleton<ChatListViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddSingleton<ContactListViewModel>();
        services.AddTransient<AddContactViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QrGeneratorViewModel>();
    }
}
