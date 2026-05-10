using FiweClient.Services.Session;

namespace FiweClient.Services.Api;

// ── Messages ──────────────────────────────────────────────────────

public interface IMessageApiService
{
    Task<IEnumerable<ChatDto>> GetAllMyChatsAsync();
    Task<string> GetMessageByIdAsync(string messageId);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(string chatId);
    Task<string?> SendMessageAsync(string chatId, string encryptedBody);
}

public class MessageApiService : BaseApiService, IMessageApiService
{
    public MessageApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public async Task<IEnumerable<ChatDto>> GetAllMyChatsAsync()
        => await GetAsync<IEnumerable<ChatDto>>("Messages/GetAllMyChats") ?? [];

    public async Task<string> GetMessageByIdAsync(string messageId)
    => await GetAsync<string>($"Messages/{messageId}/GetMessage");

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string chatId)
        => await GetAsync<IEnumerable<MessageDto>>($"Messages/{chatId}/Messages") ?? [];

    public Task<string?> SendMessageAsync(string chatId, string encryptedBody)
        => PostAsync<string>("Messages/SendMessage", new SendMessageRequest(chatId, encryptedBody));
}

// ── Contacts ──────────────────────────────────────────────────────

public interface IContactApiService
{
    Task<IEnumerable<UserSearchResult>> FindUsersAsync(string publicUserName);
    Task<string?> AddContactAsync(string userId, string contactName);
}

public class ContactApiService : BaseApiService, IContactApiService
{
    public ContactApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public async Task<IEnumerable<UserSearchResult>> FindUsersAsync(string publicUserName)
        => await PostAsync<IEnumerable<UserSearchResult>>("Contacts/FindUsers", publicUserName) ?? [];

    public Task<string?> AddContactAsync(string userId, string contactName)
        => PostAsync<string>("Contacts/AddContact", new AddContactRequest(userId, contactName));
}

// ── Users ─────────────────────────────────────────────────────────

public interface IUserApiService
{
    Task SetPublicKeyAsync(string publicKeyBase64);
    Task<string?> GetPublicKeyAsync(string userId);
}

public class UserApiService : BaseApiService, IUserApiService
{
    public UserApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public Task SetPublicKeyAsync(string publicKeyBase64)
        => PostAsync("Users/SetPublicKey", publicKeyBase64);

    public async Task<string?> GetPublicKeyAsync(string userId)
    {
        var result = await GetAsync<PublicKeyResponse>($"Users/{userId}/PublicKey");
        return result?.PublicKeyBase64;
    }
}
