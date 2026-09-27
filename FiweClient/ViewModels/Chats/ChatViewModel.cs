using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;
using FiweClient.Services.Settings;

namespace FiweClient.ViewModels.Chats;

/// <summary>
/// ViewModel открытого чата.
/// Здесь происходит всё E2EE шифрование:
///   — Отправка: plaintext → Encrypt → сервер
///   — Получение: сервер → Decrypt → отображение
/// </summary>
public partial class ChatViewModel : ObservableObject
{
    private readonly IMessageApiService _messageApi;
    private readonly IImageApiService _imageApi;
    private readonly IUserApiService _userApi;
    private readonly ISessionService _session;
    private readonly ICryptoService _crypto;
    private readonly IKeyStorageService _keyStorage;
    private readonly ISharedSecretCache _secretCache;
    private readonly IRealtimeService _realtime;
    private readonly IAppSettingsService _appSettings;

    // Маркер, которым помечается зашифрованное тело сообщения-изображения.
    // Реальные картинки в чате исключены — маловероятно столкновение с обычным текстом.
    private const string ImageMarkerPrefix = "[[fiwe-image:";
    private const string ImageMarkerSuffix = "]]";
    private const long MaxImageSizeBytes = 10 * 1024 * 1024; // должно совпадать с лимитом на сервере (ImagesController)

    // ── Состояние ─────────────────────────────────────────────────
    [ObservableProperty] private string _chatId = "";
    [ObservableProperty] private string _chatName = "";
    [ObservableProperty] private string _messageInput = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<MessageBubbleViewModel> _messages = [];
    [ObservableProperty] private bool _isSelectionMode;
    [ObservableProperty] private MessageBubbleViewModel? _replyingTo;

    public int SelectedCount => Messages.Count(m => m.IsSelected);

    /// <summary>Показывать ли панель предпросмотра ответа над полем ввода.</summary>
    public bool HasReply => ReplyingTo != null;

    partial void OnReplyingToChanged(MessageBubbleViewModel? value)
        => OnPropertyChanged(nameof(HasReply));

    // contactUserId нужен для получения публичного ключа собеседника
    private string? _contactUserId;

    public ChatViewModel(
        IMessageApiService messageApi,
        IImageApiService imageApi,
        IUserApiService userApi,
        ISessionService session,
        ICryptoService crypto,
        IKeyStorageService keyStorage,
        ISharedSecretCache secretCache,
        IRealtimeService realtime,
        IAppSettingsService appSettings)
    {
        _messageApi = messageApi;
        _imageApi = imageApi;
        _userApi = userApi;
        _session = session;
        _crypto = crypto;
        _keyStorage = keyStorage;
        _secretCache = secretCache;
        _realtime = realtime;
        _appSettings = appSettings;

        // Подписываемся на входящие сообщения от SignalR
        _realtime.MessageReceived += OnMessageReceived;

        // Отслеживаем добавление/удаление сообщений для обновления SelectedCount
        Messages.CollectionChanged += OnMessagesCollectionChanged;
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (MessageBubbleViewModel m in e.NewItems)
                m.PropertyChanged += OnMessagePropertyChanged;

        if (e.OldItems != null)
            foreach (MessageBubbleViewModel m in e.OldItems)
                m.PropertyChanged -= OnMessagePropertyChanged;

        OnPropertyChanged(nameof(SelectedCount));
    }

    private void OnMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MessageBubbleViewModel.IsSelected))
            OnPropertyChanged(nameof(SelectedCount));
    }

    partial void OnIsSelectionModeChanged(bool value)
    {
        foreach (var msg in Messages)
        {
            msg.IsSelectionMode = value;
            if (!value) msg.IsSelected = false;
        }
    }

    /// <summary>
    /// Вызывается из ChatListViewModel при открытии чата.
    /// contactUserId нужен для E2EE — чтобы найти публичный ключ собеседника.
    /// </summary>
    public void Initialize(string chatId, string chatName, string? contactUserId = null)
    {
        ChatId = chatId;
        ChatName = chatName;
        _contactUserId = contactUserId;
        Messages.Clear();
    }

    // ── Режим выделения ───────────────────────────────────────────

    [RelayCommand]
    private void EnterSelectionMode() => IsSelectionMode = true;

    [RelayCommand]
    private void CancelSelection() => IsSelectionMode = false;

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var toDelete = Messages
            .Where(m => m.IsSelected && !string.IsNullOrEmpty(m.MessageId))
            .ToList();

        if (toDelete.Count == 0)
        {
            IsSelectionMode = false;
            return;
        }

        IsBusy = true;
        try
        {
            await _messageApi.DeleteMessagesAsync(toDelete.Select(m => m.MessageId));
            foreach (var msg in toDelete)
                Messages.Remove(msg);
            IsSelectionMode = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Удаление одного сообщения — вызывается из контекстного меню
    /// (правый клик по сообщению → «Удалить»).
    /// </summary>
    [RelayCommand]
    private async Task DeleteMessageAsync(MessageBubbleViewModel? message)
    {
        if (message is null || string.IsNullOrEmpty(message.MessageId))
            return;

        IsBusy = true;
        try
        {
            await _messageApi.DeleteMessagesAsync([message.MessageId]);
            Messages.Remove(message);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Ответ на сообщение (Reply) ──────────────────────────────────

    /// <summary>
    /// Вызывается из контекстного меню (правый клик → «Ответить»).
    /// Показывает панель предпросмотра над полем ввода.
    /// </summary>
    [RelayCommand]
    private void ReplyToMessage(MessageBubbleViewModel? message)
    {
        if (message is null) return;
        ReplyingTo = message;
    }

    [RelayCommand]
    private void CancelReply() => ReplyingTo = null;

    /// <summary>
    /// Собирает короткую цитату исходного сообщения, которая добавляется
    /// перед текстом ответа (без изменений на бэкенде — просто текстовое
    /// соглашение внутри уже существующего зашифрованного тела сообщения).
    /// </summary>
    private static string BuildReplyPrefix(string quotedText)
    {
        var oneLine = quotedText.Replace('\n', ' ').Trim();
        if (oneLine.Length > 60)
            oneLine = oneLine[..60] + "…";
        return $"↩️ {oneLine}\n";
    }

    // ── Загрузка истории ──────────────────────────────────────────

    [RelayCommand]
    public async Task LoadMessagesAsync()
    {
        IsBusy = true;
        try
        {
            var sharedSecret = await GetOrComputeSharedSecretAsync();
            await _realtime.JoinChatAsync(ChatId);
            var dtos = await _messageApi.GetMessagesAsync(ChatId);

            foreach (var dto in dtos)
            {
                var decrypted = DecryptSafe(dto.MessageBody, sharedSecret);
                var imageId = TryParseImageMarker(decrypted);

                var bubble = new MessageBubbleViewModel
                {
                    MessageId = dto.MessageId ?? "",
                    Text = imageId is null ? decrypted : "📷 Фото",
                    ImageId = imageId,
                    SenderId = dto.SenderObjectId,
                    SentAt = DateTime.SpecifyKind(dto.SentAt, DateTimeKind.Utc),
                    IsMine = dto.SenderObjectId == _session.UserId,
                    UtcOffsetHours = _appSettings.UtcOffsetHours,
                };
                Messages.Add(bubble);

                if (imageId is not null)
                    _ = LoadImageIntoBubbleAsync(bubble, imageId);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Отправка сообщения ────────────────────────────────────────

    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageInput)) return;

        var plainText = MessageInput;
        var repliedTo = ReplyingTo; // запоминаем на случай ошибки отправки
        var textToSend = repliedTo != null
            ? BuildReplyPrefix(repliedTo.Text) + plainText
            : plainText;

        MessageInput = ""; // очищаем поле сразу
        ReplyingTo = null; // закрываем панель предпросмотра ответа

        try
        {
            // 1. Получаем SharedSecret (из кеша или вычисляем)
            var sharedSecret = await GetOrComputeSharedSecretAsync();

            // 2. Шифруем — на сервер уходит зашифрованный blob
            var encrypted = _crypto.Encrypt(textToSend, sharedSecret);

            // 3. Отправляем, сервер возвращает MessageId нового сообщения
            var newMessageId = await _messageApi.SendMessageAsync(ChatId, encrypted);

            // 4. Сразу показываем своё сообщение локально
            //    (MessageId сохраняем, иначе сообщение нельзя будет
            //    удалить/среагировать на него до перезагрузки чата)
            Messages.Add(new MessageBubbleViewModel
            {
                MessageId = newMessageId ?? "",
                Text = textToSend,
                SenderId = _session.UserId ?? "",
                SentAt = DateTime.UtcNow,
                IsMine = true,
                UtcOffsetHours = _appSettings.UtcOffsetHours,
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка отправки: {ex.Message}";
            MessageInput = plainText; // возвращаем текст если ошибка
            ReplyingTo = repliedTo;   // и панель предпросмотра ответа тоже
        }
    }

    // ── Отправка изображения ────────────────────────────────────────

    /// <summary>
    /// Загружает изображение на сервер (в открытом виде) и отправляет в чат
    /// зашифрованное сообщение-ссылку на него. Вызывается из code-behind ChatView
    /// после того как пользователь выбрал файл через StorageProvider.
    /// </summary>
    public async Task SendImageAsync(byte[] fileBytes, string fileName)
    {
        if (fileBytes.Length == 0) return;

        if (fileBytes.Length > MaxImageSizeBytes)
        {
            ErrorMessage = "Изображение слишком большое (максимум 10 МБ).";
            return;
        }

        var contentType = GetImageContentType(fileName);
        if (contentType is null)
        {
            ErrorMessage = "Неподдерживаемый формат изображения (допустимы JPG, PNG, GIF, WEBP, BMP).";
            return;
        }

        IsBusy = true;
        try
        {
            var sharedSecret = await GetOrComputeSharedSecretAsync();

            // Само изображение хранится на сервере без шифрования и доступно всем по ID.
            // Зашифрованным остаётся только сообщение со ссылкой на него.
            var imageId = await _imageApi.UploadImageAsync(fileBytes, fileName, contentType);

            var marker = $"{ImageMarkerPrefix}{imageId}{ImageMarkerSuffix}";
            var encryptedMarker = _crypto.Encrypt(marker, sharedSecret);
            var newMessageId = await _messageApi.SendMessageAsync(ChatId, encryptedMarker);

            var bubble = new MessageBubbleViewModel
            {
                MessageId = newMessageId ?? "",
                Text = "📷 Фото",
                ImageId = imageId,
                SenderId = _session.UserId ?? "",
                SentAt = DateTime.UtcNow,
                IsMine = true,
                UtcOffsetHours = _appSettings.UtcOffsetHours,
                ImageBitmap = LoadBitmap(fileBytes), // байты уже есть локально — сразу показываем, без похода на сервер
            };
            Messages.Add(bubble);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка отправки изображения: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Входящие сообщения через SignalR ──────────────────────────

    private async void OnMessageReceived(string chatId, IEnumerable<string> recipients, string messageId)
    {
        // Фильтруем — только сообщения в текущий открытый чат
        // if (chatId != ChatId) return;

        // Своё сообщение уже добавлено локально — пропускаем
        if (!recipients.Contains(_session.UserId)) return;

        try
        {
            var sharedSecret = await GetOrComputeSharedSecretAsync();
            var encripted = await _messageApi.GetMessageByIdAsync(messageId);
            var decrypted = DecryptSafe(encripted, sharedSecret);
            var imageId = TryParseImageMarker(decrypted);

            // UI поток — Avalonia требует обновления коллекций из UI потока
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var bubble = new MessageBubbleViewModel
                {
                    Text = imageId is null ? decrypted : "📷 Фото",
                    ImageId = imageId,
                    SenderId = "",
                    SentAt = DateTime.UtcNow,
                    IsMine = false,
                    UtcOffsetHours = _appSettings.UtcOffsetHours,
                };
                Messages.Add(bubble);

                if (imageId is not null)
                    _ = LoadImageIntoBubbleAsync(bubble, imageId);
            });
        }
        catch
        {
            // Не удалось расшифровать — игнорируем сообщение
        }
    }

    /// <summary>
    /// Скачивает изображение с сервера и декодирует в Bitmap.
    /// Не бросает исключений наружу — при ошибке пузырь остаётся с текстовым плейсхолдером.
    /// </summary>
    private async Task LoadImageIntoBubbleAsync(MessageBubbleViewModel bubble, string imageId)
    {
        bubble.IsImageLoading = true;
        try
        {
            var imageBytes = await _imageApi.DownloadImageAsync(imageId);
            var bitmap = LoadBitmap(imageBytes);

            Avalonia.Threading.Dispatcher.UIThread.Post(() => bubble.ImageBitmap = bitmap);
        }
        catch
        {
            // Не удалось скачать/расшифровать — оставляем текстовый плейсхолдер
        }
        finally
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => bubble.IsImageLoading = false);
        }
    }

    private static Bitmap LoadBitmap(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return new Bitmap(ms);
    }

    /// <summary>MIME-тип по расширению файла; null — если формат не поддерживается сервером.</summary>
    private static string? GetImageContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => null,
        };

    private static string? TryParseImageMarker(string text)
    {
        if (text.StartsWith(ImageMarkerPrefix, StringComparison.Ordinal) &&
            text.EndsWith(ImageMarkerSuffix, StringComparison.Ordinal))
        {
            return text[ImageMarkerPrefix.Length..^ImageMarkerSuffix.Length];
        }
        return null;
    }

    // ── Криптография ──────────────────────────────────────────────

    /// <summary>
    /// Получает SharedSecret из кеша или вычисляет через ECDH.
    /// 
    /// Алгоритм:
    /// 1. Проверяем кеш (contactUserId → sharedSecret)
    /// 2. Если нет — берём свой приватный ключ с диска
    /// 3. Берём публичный ключ собеседника с сервера
    /// 4. ECDH → SharedSecret → кладём в кеш
    /// </summary>
    private async Task<byte[]> GetOrComputeSharedSecretAsync()
    {
        var contactId = _contactUserId ?? throw new InvalidOperationException("contactUserId not set");

        // Проверяем кеш
        if (_secretCache.TryGet(contactId, out var cached))
            return cached;

        // Загружаем свой приватный ключ
        // Пароль для расшифровки keystore = пароль от аккаунта (хранится в сессии)
        var keyPair = await _keyStorage.LoadKeyPairAsync(_session.UserId!)
            ?? throw new Exception("Не удалось загрузить ключевую пару");

        // Получаем публичный ключ собеседника
        var contactPublicKeyBase64 = await _userApi.GetPublicKeyAsync(contactId)
            ?? throw new Exception($"Публичный ключ пользователя {contactId} не найден");

        // Вычисляем SharedSecret через ECDH
        var sharedSecret = _crypto.ComputeSharedSecret(
            keyPair.PrivateKey,
            Convert.FromBase64String(contactPublicKeyBase64)
        );

        // Кешируем на время сессии
        _secretCache.Set(contactId, sharedSecret);

        return sharedSecret;
    }

    /// <summary>
    /// Безопасная расшифровка — если не удалось, показываем плейсхолдер.
    /// </summary>
    private string DecryptSafe(string encryptedBase64, byte[] sharedSecret)
    {
        try
        {
            return _crypto.Decrypt(encryptedBase64, sharedSecret);
        }
        catch
        {
            return "[не удалось расшифровать]";
        }
    }
}