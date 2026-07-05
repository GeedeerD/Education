namespace FiweClient.Services.Api;

// ── Auth ──────────────────────────────────────────────────────────

public record LoginRequest(string UserName, string Password);
public record RegisterRequest(string UserName, string Password);
public record LoginResponse(string Token);

// ── Chats ─────────────────────────────────────────────────────────

public record ChatDto(
    string ChatId,
    string Name,
    string? PictureUrl,
    DateTime LastActiveDateTime,
    string? LastMessagePreview,
    string? RecipientId
);

// ── Messages ──────────────────────────────────────────────────────

public record MessageDto(
    DateTime SentAt,
    string SenderObjectId,
    string MessageBody,         // на сервере — зашифрованный base64
    string? MessageId = null
);

public record SendMessageRequest(string ChatId, string MessageBody);
public record DeleteMessagesRequest(List<string> MessageIds);

// ── Contacts ──────────────────────────────────────────────────────

public record UserSearchResult(string UserId, string UserName);
public record AddContactRequest(string UserId, string ContactName);

// ── Keys ──────────────────────────────────────────────────────────

public record SetPublicKeyRequest(string PublicKeyBase64);
public record PublicKeyResponse(string PublicKeyBase64);
