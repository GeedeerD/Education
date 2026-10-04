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

    // Маркер, которым помечается зашифрованное тело сообщения с изображениями:
    //   [[fiwe-image:id1;id2;...]]\nподпись
    // Сообщения старого формата (один id, без подписи) разбираются этим же кодом.
    private const string ImageMarkerPrefix = "[[fiwe-image:";
    private const string ImageMarkerSuffix = "]]";
    private const char ImageIdSeparator = ';';
    private const long MaxImageSizeBytes = 10 * 1024 * 1024; // должно совпадать с лимитом на сервере (ImagesController)
    public const int MaxAttachmentsPerMessage = 10;
    private const int AttachmentThumbnailWidth = 160;

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

    /// <summary>Изображения, выбранные для отправки (показываются над полем ввода до нажатия «Отправить»).</summary>
    public ObservableCollection<PendingAttachmentViewModel> PendingAttachments { get; } = [];
    public bool HasPendingAttachments => PendingAttachments.Count > 0;

    // ── Просмотр изображения на весь экран ──
    [ObservableProperty] private MessageImageViewModel? _viewedImage;
    private IReadOnlyList<MessageImageViewModel> _viewerImages = [];

    public bool IsImageViewerOpen => ViewedImage != null;
    public bool CanViewerNavigate => _viewerImages.Count > 1;
    public string ViewerPositionLabel => ViewedImage is null
        ? ""
        : $"{IndexOfViewed() + 1} / {_viewerImages.Count}";

    partial void OnViewedImageChanged(MessageImageViewModel? value)
    {
        OnPropertyChanged(nameof(IsImageViewerOpen));
        OnPropertyChanged(nameof(CanViewerNavigate));
        OnPropertyChanged(nameof(ViewerPositionLabel));
    }

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

        PendingAttachments.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPendingAttachments));
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
        PendingAttachments.Clear();
        CloseImageViewer();
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
                var bubble = CreateBubble(
                    dto.MessageId ?? "",
                    decrypted,
                    dto.SenderObjectId,
                    DateTime.SpecifyKind(dto.SentAt, DateTimeKind.Utc),
                    isMine: dto.SenderObjectId == _session.UserId);
                Messages.Add(bubble);
                LoadBubbleImages(bubble);
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

    /// <summary>
    /// Отправляет то, что набрано в поле ввода. Если выбраны изображения —
    /// они уходят одним сообщением, а набранный текст становится подписью.
    /// </summary>
    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (PendingAttachments.Count > 0)
        {
            await SendAttachmentsAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(MessageInput)) return;

        var plainText = MessageInput;
        var repliedTo = ReplyingTo; // запоминаем на случай ошибки отправки
        var textToSend = repliedTo != null
            ? BuildReplyPrefix(repliedTo.PreviewText) + plainText
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
            Messages.Add(CreateBubble(newMessageId ?? "", textToSend, _session.UserId ?? "", DateTime.UtcNow, isMine: true));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка отправки: {ex.Message}";
            MessageInput = plainText; // возвращаем текст если ошибка
            ReplyingTo = repliedTo;   // и панель предпросмотра ответа тоже
        }
    }

    // ── Вложения-изображения ───────────────────────────────────────

    /// <summary>
    /// Добавляет выбранный пользователем файл в список вложений (без отправки).
    /// Вызывается из code-behind ChatView после выбора файлов через StorageProvider.
    /// </summary>
    public void AddAttachment(byte[] fileBytes, string fileName)
    {
        if (fileBytes.Length == 0) return;

        if (PendingAttachments.Count >= MaxAttachmentsPerMessage)
        {
            ErrorMessage = $"Можно прикрепить не больше {MaxAttachmentsPerMessage} изображений.";
            return;
        }

        if (fileBytes.Length > MaxImageSizeBytes)
        {
            ErrorMessage = $"«{fileName}» слишком большое (максимум 10 МБ).";
            return;
        }

        var contentType = GetImageContentType(fileName);
        if (contentType is null)
        {
            ErrorMessage = $"«{fileName}»: неподдерживаемый формат (допустимы JPG, PNG, GIF, WEBP, BMP).";
            return;
        }

        Bitmap thumbnail;
        try
        {
            using var ms = new MemoryStream(fileBytes);
            thumbnail = Bitmap.DecodeToWidth(ms, AttachmentThumbnailWidth);
        }
        catch
        {
            ErrorMessage = $"«{fileName}» не удалось открыть как изображение.";
            return;
        }

        PendingAttachments.Add(new PendingAttachmentViewModel
        {
            Bytes = fileBytes,
            FileName = fileName,
            ContentType = contentType,
            Thumbnail = thumbnail,
        });
    }

    [RelayCommand]
    private void RemoveAttachment(PendingAttachmentViewModel? attachment)
    {
        if (attachment is not null)
            PendingAttachments.Remove(attachment);
    }

    /// <summary>
    /// Загружает изображения на сервер (в открытом виде) и отправляет в чат
    /// одно зашифрованное сообщение со ссылками на них и подписью.
    /// </summary>
    private async Task SendAttachmentsAsync()
    {
        if (IsBusy) return; // защита от повторной отправки по Enter, пока идёт загрузка

        var attachments = PendingAttachments.ToList();
        var caption = MessageInput.Trim();
        var repliedTo = ReplyingTo;
        if (repliedTo != null)
            caption = BuildReplyPrefix(repliedTo.PreviewText) + caption;

        IsBusy = true;
        try
        {
            var sharedSecret = await GetOrComputeSharedSecretAsync();

            // Сами изображения хранятся на сервере без шифрования и доступны всем по ID.
            // Зашифрованным остаётся только сообщение со ссылками на них.
            var imageIds = new List<string>(attachments.Count);
            foreach (var attachment in attachments)
                imageIds.Add(await _imageApi.UploadImageAsync(attachment.Bytes, attachment.FileName, attachment.ContentType));

            var body = BuildImageMessage(imageIds, caption);
            var newMessageId = await _messageApi.SendMessageAsync(ChatId, _crypto.Encrypt(body, sharedSecret));

            var bubble = CreateBubble(newMessageId ?? "", body, _session.UserId ?? "", DateTime.UtcNow, isMine: true);
            // байты уже есть локально — сразу показываем, без похода на сервер
            for (var i = 0; i < bubble.Images.Count; i++)
                bubble.Images[i].Bitmap = LoadBitmap(attachments[i].Bytes);
            Messages.Add(bubble);

            // Очищаем ввод только после успешной отправки — при ошибке всё остаётся на месте
            PendingAttachments.Clear();
            MessageInput = "";
            ReplyingTo = null;
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

    // ── Просмотр изображения ────────────────────────────────────────

    /// <summary>Открывает изображение на весь экран; стрелками листаются остальные картинки того же сообщения.</summary>
    public void OpenImageViewer(MessageBubbleViewModel bubble, MessageImageViewModel image)
    {
        if (IsSelectionMode || !image.HasBitmap) return;

        _viewerImages = bubble.Images;
        ViewedImage = image;
    }

    [RelayCommand]
    private void CloseImageViewer()
    {
        ViewedImage = null;
        _viewerImages = [];
    }

    [RelayCommand]
    private void ShowNextImage() => MoveViewer(+1);

    [RelayCommand]
    private void ShowPreviousImage() => MoveViewer(-1);

    private void MoveViewer(int step)
    {
        if (ViewedImage is null || _viewerImages.Count < 2) return;

        var count = _viewerImages.Count;
        ViewedImage = _viewerImages[(IndexOfViewed() + step + count) % count];
    }

    private int IndexOfViewed()
    {
        for (var i = 0; i < _viewerImages.Count; i++)
            if (ReferenceEquals(_viewerImages[i], ViewedImage))
                return i;
        return 0;
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

            // UI поток — Avalonia требует обновления коллекций из UI потока
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var bubble = CreateBubble("", decrypted, "", DateTime.UtcNow, isMine: false);
                Messages.Add(bubble);
                LoadBubbleImages(bubble);
            });
        }
        catch
        {
            // Не удалось расшифровать — игнорируем сообщение
        }
    }

    /// <summary>Создаёт пузырь из расшифрованного тела: распознаёт сообщения с изображениями.</summary>
    private MessageBubbleViewModel CreateBubble(string messageId, string decrypted, string senderId, DateTime sentAt, bool isMine)
    {
        var isImageMessage = TryParseImageMessage(decrypted, out var imageIds, out var caption);

        return new MessageBubbleViewModel
        {
            MessageId = messageId,
            Text = isImageMessage ? caption : decrypted,
            Images = imageIds.Select(id => new MessageImageViewModel { ImageId = id }).ToList(),
            SenderId = senderId,
            SentAt = sentAt,
            IsMine = isMine,
            UtcOffsetHours = _appSettings.UtcOffsetHours,
        };
    }

    private void LoadBubbleImages(MessageBubbleViewModel bubble)
    {
        foreach (var image in bubble.Images)
            _ = LoadImageAsync(image);
    }

    /// <summary>
    /// Скачивает изображение с сервера и декодирует в Bitmap.
    /// Не бросает исключений наружу — при ошибке остаётся плейсхолдер.
    /// </summary>
    private async Task LoadImageAsync(MessageImageViewModel image)
    {
        image.IsLoading = true;
        try
        {
            var imageBytes = await _imageApi.DownloadImageAsync(image.ImageId);
            var bitmap = LoadBitmap(imageBytes);

            Avalonia.Threading.Dispatcher.UIThread.Post(() => image.Bitmap = bitmap);
        }
        catch
        {
            // Не удалось скачать — оставляем плейсхолдер
        }
        finally
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => image.IsLoading = false);
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

    private static string BuildImageMessage(IEnumerable<string> imageIds, string caption)
    {
        var marker = ImageMarkerPrefix + string.Join(ImageIdSeparator, imageIds) + ImageMarkerSuffix;
        return string.IsNullOrEmpty(caption) ? marker : $"{marker}\n{caption}";
    }

    private static bool TryParseImageMessage(string text, out string[] imageIds, out string caption)
    {
        imageIds = [];
        caption = "";

        if (!text.StartsWith(ImageMarkerPrefix, StringComparison.Ordinal))
            return false;

        var end = text.IndexOf(ImageMarkerSuffix, ImageMarkerPrefix.Length, StringComparison.Ordinal);
        if (end < 0)
            return false;

        var ids = text[ImageMarkerPrefix.Length..end]
            .Split(ImageIdSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (ids.Length == 0)
            return false;

        imageIds = ids;
        var rest = text[(end + ImageMarkerSuffix.Length)..];
        caption = rest.StartsWith('\n') ? rest[1..] : rest;
        return true;
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