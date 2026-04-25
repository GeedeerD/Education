using Avalonia;
using Avalonia.Fonts.Inter;
using FiweClient;

AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    //.UseInterFont()
    .LogToTrace()
    .StartWithClassicDesktopLifetime(args);
