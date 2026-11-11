using UIKit;

namespace FiweClient.iOS;

public static class Application
{
    // Точка входа iOS-приложения: UIKit сам создаёт AppDelegate, а он — Avalonia App
    private static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}
