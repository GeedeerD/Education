using System.Text.Json;

namespace FiweClient.Services.Session;

/// <summary>
/// Сохраняет JWT токен на диск чтобы не логиниться каждый раз.
/// Файл: %AppData%/Fiwe/session.json
/// </summary>
public interface ITokenStorage
{
    Task SaveTokenAsync(string token, string userId, string username);
    Task<SavedSession?> LoadTokenAsync();
    Task ClearAsync();
}

public record SavedSession(string Token, string UserId, string Username, DateTime SavedAt);

public class TokenStorage : ITokenStorage
{
    private readonly string _path;

    public TokenStorage()
    {
        var dir = GetStorageDir();
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "session.json");
    }

    public async Task SaveTokenAsync(string token, string userId, string username)
    {
        var session = new SavedSession(token, userId, username, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(session);
        await File.WriteAllTextAsync(_path, json);
    }

    public async Task<SavedSession?> LoadTokenAsync()
    {
        if (!File.Exists(_path)) return null;
        try
        {
            var json = await File.ReadAllTextAsync(_path);
            return JsonSerializer.Deserialize<SavedSession>(json);
        }
        catch { return null; }
    }

    public async Task ClearAsync()
    {
        if (File.Exists(_path))
            File.Delete(_path);
        await Task.CompletedTask;
    }

    private static string GetStorageDir()
    {
        //return Environment.SpecialFolder.ApplicationData;
#if ANDROID
    return Android.App.Application.Context.FilesDir!.AbsolutePath;
#else
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiwe");
#endif
    }
}
