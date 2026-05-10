using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Crypto;
using FiweClient.Services.Api;
using FiweClient.Services.Realtime;
using FiweClient.Services.Session;

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
    private readonly IUserApiService _userApi;
    private readonly ISessionService _session;
    private readonly ICryptoService _crypto;
    private readonly IKeyStorageService _keyStorage;
    private readonly ISharedSecretCache _secretCache;
    private readonly IRealtimeService _realtime;

    // ── Состояние ─────────────────────────────────────────────────
    [ObservableProperty] private string _chatId = "";
    [ObservableProperty] private string _chatName = "";
    [ObservableProperty] private string _messageInput = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<MessageBubbleViewModel> _messages = [];

    // contactUserId нужен для получения публичного ключа собеседника
    private string? _contactUserId;

    public ChatViewModel(
        IMessageApiService messageApi,
        IUserApiService userApi,
        ISessionService session,
        ICryptoService crypto,
        IKeyStorageService keyStorage,
        ISharedSecretCache secretCache,
        IRealtimeService realtime)
    {
        _messageApi = messageApi;
        _userApi = userApi;
        _session = session;
        _crypto = crypto;
        _keyStorage = keyStorage;
        _secretCache = secretCache;
        _realtime = realtime;

        // Подписываемся на входящие сообщения от SignalR
        _realtime.MessageReceived += OnMessageReceived;
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
                Messages.Add(new MessageBubbleViewModel
                {
                    Text = decrypted,
                    SenderId = dto.SenderObjectId,
                    SentAt = dto.SentAt,
                    IsMine = dto.SenderObjectId == _session.UserId,
                });
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
        MessageInput = ""; // очищаем поле сразу

        try
        {
            // 1. Получаем SharedSecret (из кеша или вычисляем)
            var sharedSecret = await GetOrComputeSharedSecretAsync();

            // 2. Шифруем — на сервер уходит зашифрованный blob
            var encrypted = _crypto.Encrypt(plainText, sharedSecret);

            // 3. Отправляем
            await _messageApi.SendMessageAsync(ChatId, encrypted);

            // 4. Сразу показываем своё сообщение локально
            Messages.Add(new MessageBubbleViewModel
            {
                Text = plainText,
                SenderId = _session.UserId ?? "",
                SentAt = DateTime.Now,
                IsMine = true,
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка отправки: {ex.Message}";
            MessageInput = plainText; // возвращаем текст если ошибка
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
            //var decrypted = DecryptSafe(encryptedBody, sharedSecret);

            // UI поток — Avalonia требует обновления коллекций из UI потока
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Messages.Add(new MessageBubbleViewModel
                {
                    Text = "decrypted",
                    SenderId = "",
                    SentAt = DateTime.UtcNow,
                    IsMine = false,
                });
            });
        }
        catch
        {
            // Не удалось расшифровать — игнорируем сообщение
        }
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
        var keyPair = await _keyStorage.LoadKeyPairAsync(_session.Token!)
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
