using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Media.Imaging;
using FiweClient.Services.Api;

namespace FiweClient.Services.Avatars;

/// <summary>
/// Аватары пользователей (issue #42): скачивание с кэшем и подготовка своей фотографии к загрузке.
/// Аватары — публичные картинки на сервере (как и вложения), поэтому не шифруются.
/// </summary>
public interface IAvatarService
{
    /// <summary>Картинка аватара по относительному URL от сервера; null — URL пустой или скачать не удалось.</summary>
    Task<Bitmap?> GetAsync(string? avatarUrl);

    /// <summary>Положить уже известную картинку в кэш (свой новый аватар — чтобы не скачивать его заново).</summary>
    void Put(string avatarUrl, Bitmap bitmap);
}

public class AvatarService : IAvatarService
{
    /// <summary>Сторона квадратного аватара в пикселях: хватает для 64px на экранах с плотностью 3x и весит немного.</summary>
    public const int AvatarSize = 320;

    private readonly IImageApiService _imageApi;

    // Кэшируем задачу, а не результат: несколько строк списка с одним аватаром не скачают его по нескольку раз
    private readonly ConcurrentDictionary<string, Task<Bitmap?>> _cache = new();

    public AvatarService(IImageApiService imageApi) => _imageApi = imageApi;

    public Task<Bitmap?> GetAsync(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return Task.FromResult<Bitmap?>(null);

        return _cache.GetOrAdd(avatarUrl, DownloadAsync);
    }

    public void Put(string avatarUrl, Bitmap bitmap) => _cache[avatarUrl] = Task.FromResult<Bitmap?>(bitmap);

    private async Task<Bitmap?> DownloadAsync(string avatarUrl)
    {
        try
        {
            var bytes = await _imageApi.DownloadByUrlAsync(avatarUrl);
            using var ms = new MemoryStream(bytes);
            return Bitmap.DecodeToWidth(ms, AvatarSize);
        }
        catch
        {
            // Не скачалось (нет сети, картинку удалили) — показываем инициалы и попробуем в следующий раз
            _cache.TryRemove(avatarUrl, out _);
            return null;
        }
    }

    /// <summary>
    /// Обрезает фото по центру до квадрата и уменьшает до <see cref="AvatarSize"/>×<see cref="AvatarSize"/> PNG.
    /// Так на сервер уходит ~100 КБ вместо многомегабайтного снимка с камеры.
    /// Вызывать из UI-потока (рисование через RenderTargetBitmap).
    /// </summary>
    public static (byte[] Png, Bitmap Preview) PrepareForUpload(byte[] sourceImage)
    {
        using var input = new MemoryStream(sourceImage);
        using var original = new Bitmap(input);

        var side = Math.Min(original.Size.Width, original.Size.Height);
        var sourceRect = new Rect(
            (original.Size.Width - side) / 2,
            (original.Size.Height - side) / 2,
            side,
            side);

        var target = new RenderTargetBitmap(new PixelSize(AvatarSize, AvatarSize), new Vector(96, 96));
        using (var context = target.CreateDrawingContext())
            context.DrawImage(original, sourceRect, new Rect(0, 0, AvatarSize, AvatarSize));

        using var output = new MemoryStream();
        target.Save(output);
        return (output.ToArray(), target);
    }
}
