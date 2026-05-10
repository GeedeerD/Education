using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;
using FiweClient.ViewModels;
using FiweClient.ViewModels.Auth;
using FiweClient.ViewModels.Chats;
using FiweClient.ViewModels.Contacts;
using FiweClient.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FiweClient;

public class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };

            var nav = Services.GetRequiredService<INavigationService>();
            nav.SetHost(mainWindow);

            desktop.MainWindow = mainWindow;

            // Запускаем авто-логин асинхронно
            _ = TryAutoLoginAsync();
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

        var saved = await tokenStorage.LoadTokenAsync();

        if (saved is not null && session.IsTokenValid(saved.Token))
        {
            // Токен ещё живой — восстанавливаем сессию
            session.SetSession(saved.Token, saved.UserId, saved.Username);
            await realtime.ConnectAsync(saved.Token);

            var shell = Services.GetRequiredService<ShellViewModel>();
            shell.Initialize();
            nav.NavigateTo(shell);
        }
        else
        {
            // Токен истёк или нет — на логин
            await tokenStorage.ClearAsync();
            nav.NavigateTo<LoginViewModel>();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // ── HTTP ──────────────────────────────────────
        services.AddHttpClient("FiweApi", client =>
        {
            client.BaseAddress = new Uri("https://8073-91-247-76-23.ngrok-free.app/");
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

        // ── Навигация ─────────────────────────────────
        services.AddSingleton<INavigationService, NavigationService>();

        // ── ViewModels ────────────────────────────────
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ShellViewModel>();       // singleton — одна оболочка
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddSingleton<ChatListViewModel>();    // singleton — не перезагружать список
        services.AddTransient<ChatViewModel>();
        services.AddSingleton<ContactListViewModel>();
        services.AddTransient<AddContactViewModel>();
    }
}
