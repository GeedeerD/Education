using FiweClient.Services.Session;
using Microsoft.AspNetCore.SignalR.Client;

namespace FiweClient.Services.Realtime;

/// <summary>
/// Реалтайм события от сервера через SignalR WebSocket.
/// </summary>
public interface IRealtimeService
{
    /// <summary>Новое зашифрованное сообщение пришло в чат</summary>
    event Action<string, string, string>? MessageReceived; // chatId, senderId, encryptedBody

    Task ConnectAsync(string token);
    Task DisconnectAsync();
}

public class SignalRService : IRealtimeService, IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly ISessionService _session;

    public event Action<string, string, string>? MessageReceived;

    public SignalRService(ISessionService session)
    {
        _session = session;
    }

    public async Task ConnectAsync(string token)
    {
        // Если уже подключены — ничего не делаем
        if (_connection?.State == HubConnectionState.Connected)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7187/chatHub", options =>
            {
                // Передаём JWT токен как query параметр —
                // стандартный способ авторизации в SignalR
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect() // авто-переподключение при обрыве
            .Build();

        // Слушаем событие "ReceiveMessage" от сервера
        // Сервер должен вызывать: Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, senderId, encryptedBody)
        _connection.On<string, string, string>("ReceiveMessage",
            (chatId, senderId, encryptedBody) =>
            {
                MessageReceived?.Invoke(chatId, senderId, encryptedBody);
            });

        _connection.Reconnected += async _ =>
        {
            // После переподключения — присоединяемся к своим чатам снова
            await RejoinChatsAsync();
        };

        await _connection.StartAsync();
    }

    /// <summary>
    /// Присоединяет пользователя к SignalR группе чата.
    /// Вызывается при открытии чата.
    /// </summary>
    public async Task JoinChatAsync(string chatId)
    {
        if (_connection?.State != HubConnectionState.Connected) return;
        await _connection.InvokeAsync("JoinChat", chatId);
    }

    public async Task DisconnectAsync()
    {
        if (_connection is not null)
            await _connection.StopAsync();
    }

    private async Task RejoinChatsAsync()
    {
        // TODO: хранить список активных чатов и переподключаться
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
