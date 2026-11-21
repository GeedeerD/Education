using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FiweClient.ViewModels;

namespace FiweClient.Views;

public partial class SettingsView : UserControl
{
    public SettingsView() => InitializeComponent();

    /// <summary>
    /// «Фото» / клик по аватару: системный выбор картинки (работает и на десктопе, и на телефоне),
    /// дальше ViewModel обрезает её до квадрата и загружает на сервер.
    /// </summary>
    private async void OnChangeAvatarClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm)
            return;

        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите фото профиля",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });

        if (files.Count == 0)
            return;

        await using var stream = await files[0].OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        await vm.ChangeAvatarAsync(ms.ToArray());
    }
}
