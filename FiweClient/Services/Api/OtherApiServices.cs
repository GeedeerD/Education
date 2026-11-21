using FiweClient.Services.Session;

namespace FiweClient.Services.Api;

// ── Messages ──────────────────────────────────────────────────────

public interface IMessageApiService
{
    Task<IEnumerable<ChatDto>> GetAllMyChatsAsync();
    Task<string> GetMessageByIdAsync(string messageId);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(string chatId);
    Task<string?> SendMessageAsync(string chatId, string encryptedBody);
    Task RemoveOldMessagesAsync();
    Task DeleteMessagesAsync(IEnumerable<string> messageIds);
}

public class MessageApiService : BaseApiService, IMessageApiService
{
    public MessageApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public async Task<IEnumerable<ChatDto>> GetAllMyChatsAsync()
        => await GetAsync<IEnumerable<ChatDto>>("Messages/GetAllMyChats") ?? [];

    public async Task<string    > GetMessageByIdAsync(string messageId)
    => await GetAsync<string>($"Messages/{messageId}/GetMessage");

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string chatId)
        => await GetAsync<IEnumerable<MessageDto>>($"Messages/{chatId}/Messages") ?? [];

    public Task<string?> SendMessageAsync(string chatId, string encryptedBody)
        => PostAsync<string>("Messages/SendMessage", new SendMessageRequest(chatId, encryptedBody));

    public Task RemoveOldMessagesAsync()
        => PostAsync("Messages/RemoveOldMessages", null);

    public Task DeleteMessagesAsync(IEnumerable<string> messageIds)
        => PostAsync("Messages/DeleteMessages", new DeleteMessagesRequest(messageIds.ToList()));
}

// ── Images ────────────────────────────────────────────────────────

public interface IImageApiService
{
    Task<string> UploadImageAsync(byte[] fileBytes, string fileName, string contentType);
    Task<byte[]> DownloadImageAsync(string imageId);

    /// <summary>Скачать картинку по относительному URL от сервера (аватары: "Images/{id}/Download").</summary>
    Task<byte[]> DownloadByUrlAsync(string relativeUrl);
}

public class ImageApiService : BaseApiService, IImageApiService
{
    public ImageApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public async Task<string> UploadImageAsync(byte[] fileBytes, string fileName, string contentType)
    {
        var result = await PostFileAsync<UploadImageResponse>("Images/Upload", fileBytes, fileName, contentType);
        return result?.ImageId ?? throw new Exception("Сервер не вернул ImageId загруженного изображения");
    }

    public Task<byte[]> DownloadImageAsync(string imageId)
        => GetBytesAsync($"Images/{imageId}/Download");

    public Task<byte[]> DownloadByUrlAsync(string relativeUrl)
        => GetBytesAsync(relativeUrl.TrimStart('/'));
}

// ── Contacts ──────────────────────────────────────────────────────

public interface IContactApiService
{
    Task<IEnumerable<UserSearchResult>> FindUsersAsync(string publicUserName);
    Task<string?> AddContactAsync(string userId, string contactName);

    /// <summary>Удалить контакт: его диалог пропадает из моего списка (у собеседника остаётся).</summary>
    Task DeleteContactAsync(string chatId);
}

public class ContactApiService : BaseApiService, IContactApiService
{
    public ContactApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public async Task<IEnumerable<UserSearchResult>> FindUsersAsync(string publicUserName)
        => await PostAsync<IEnumerable<UserSearchResult>>("Contacts/FindUsers", publicUserName) ?? [];

    public Task<string?> AddContactAsync(string userId, string contactName)
        => PostAsync<string>("Contacts/AddContact", new AddContactRequest(userId, contactName));

    public Task DeleteContactAsync(string chatId)
        => PostAsync("Contacts/DeleteContact", chatId);
}

// ── Users ─────────────────────────────────────────────────────────

public interface IUserApiService
{
    Task SetPublicKeyAsync(string publicKeyBase64);
    Task<string?> GetPublicKeyAsync(string userId);
    Task SetPublicNameAsync(string publicName);

    /// <summary>Сделать загруженное изображение своим аватаром. Возвращает новый URL аватара.</summary>
    Task<string?> SetAvatarAsync(string imageId);
    Task RemoveAvatarAsync();

    /// <summary>URL аватара пользователя (своего — по своему userId) или null, если аватара нет.</summary>
    Task<string?> GetAvatarUrlAsync(string userId);
}

public class UserApiService : BaseApiService, IUserApiService
{
    public UserApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public Task SetPublicKeyAsync(string publicKeyBase64)
        => PostAsync("Users/SetPublicKey", publicKeyBase64);

    public Task SetPublicNameAsync(string publicName)
        => PostAsync("Users/ChangePublicName", publicName);

    public async Task<string?> GetPublicKeyAsync(string userId)
    {
        var result = await GetAsync<PublicKeyResponse>($"Users/{userId}/PublicKey");
        return result?.PublicKeyBase64;
    }

    public async Task<string?> SetAvatarAsync(string imageId)
        => (await PostAsync<AvatarResponse>("Users/SetAvatar", imageId))?.AvatarUrl;

    public Task RemoveAvatarAsync()
        => PostAsync("Users/RemoveAvatar", new { });

    public async Task<string?> GetAvatarUrlAsync(string userId)
        => (await GetAsync<AvatarResponse>($"Users/{userId}/Avatar"))?.AvatarUrl;
}
