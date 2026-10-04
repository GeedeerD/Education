using System.Text.Json;

namespace FiweClient.Services.Settings;

public interface IAppSettingsService
{
    double UtcOffsetHours { get; set; }

    /// <summary>Форматировать отправляемые сообщения как Markdown (переключатель M↓ в чате и пункт в настройках).</summary>
    bool MarkdownEnabled { get; set; }
    Task LoadAsync();
    Task SaveAsync();
}

public class AppSettingsService : IAppSettingsService
{
    private readonly string _path;

    public double UtcOffsetHours { get; set; } =
        TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours;

    public bool MarkdownEnabled { get; set; } = true;

    public AppSettingsService()
    {
        var dir = GetStorageDir();
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "appsettings.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_path)) return;
        try
        {
            var json = await File.ReadAllTextAsync(_path);
            var dto = JsonSerializer.Deserialize<SettingsDto>(json);
            if (dto is not null)
            {
                UtcOffsetHours = dto.UtcOffsetHours;
                MarkdownEnabled = dto.MarkdownEnabled;
            }
        }
        catch { }
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(new SettingsDto(UtcOffsetHours, MarkdownEnabled));
        await File.WriteAllTextAsync(_path, json);
    }

    private static string GetStorageDir()
    {
#if ANDROID
        return Android.App.Application.Context.FilesDir!.AbsolutePath;
#else
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiwe");
#endif
    }

    // Значение по умолчанию нужно для старых appsettings.json, где MarkdownEnabled ещё нет
    private record SettingsDto(double UtcOffsetHours, bool MarkdownEnabled = true);
}
