using System.Text.Json;

namespace FiweClient.Services.Folders;

/// <summary>Папка чатов (issue #41): имя и id чатов, которые в неё входят.</summary>
public class ChatFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public List<string> ChatIds { get; set; } = [];
}

/// <summary>
/// Хранит папки чатов на устройстве — отдельно для каждого аккаунта (folders-{userId}.json).
/// На сервер папки не уходят: это личная организация списка, серверу о ней знать незачем.
/// </summary>
public interface IChatFolderService
{
    Task<List<ChatFolder>> LoadAsync(string userId);
    Task SaveAsync(string userId, IEnumerable<ChatFolder> folders);
}

public class ChatFolderService : IChatFolderService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<List<ChatFolder>> LoadAsync(string userId)
    {
        var path = GetPath(userId);
        if (!File.Exists(path))
            return [];

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<ChatFolder>>(json) ?? [];
        }
        catch
        {
            // Повреждённый файл не должен ломать список чатов — просто начинаем без папок
            return [];
        }
    }

    public async Task SaveAsync(string userId, IEnumerable<ChatFolder> folders)
    {
        var json = JsonSerializer.Serialize(folders, JsonOptions);
        await File.WriteAllTextAsync(GetPath(userId), json);
    }

    private static string GetPath(string userId)
    {
        var dir = GetStorageDir();
        Directory.CreateDirectory(dir);

        // userId — ObjectId с сервера, но на всякий случай не даём ему выйти за пределы папки
        var safeId = string.Concat(userId.Where(char.IsLetterOrDigit));
        return Path.Combine(dir, $"folders-{safeId}.json");
    }

    private static string GetStorageDir()
    {
#if ANDROID
        return Android.App.Application.Context.FilesDir!.AbsolutePath;
#else
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiwe");
#endif
    }
}
