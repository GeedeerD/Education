using DataBase.Models;
using MongoDB.Bson;

namespace DataBase.Interfaces
{
    public interface IMessageRepository
    {
        Task CreateChatAsync(ObjectId userId, ChatModelDto chat);
        Task<IEnumerable<ChatModelDto>> GetChatsAsync(ObjectId userId);
        Task SendMessageAsync(MessageModelDto message);
        Task<MessageModelDto?> ReadMessageAsync(ObjectId userId, ObjectId messageId);
        Task<ChatModelDto> GetChatByUserOjectIdsAsync(ObjectId userId, ObjectId contactUserId);
        Task<IEnumerable<MessageModelDto>> GetMessagesAsync(ObjectId userId, ObjectId chatId, int? messageCount = null);
        Task<AccessInfo> GetChatAccessInfoAsync(string userId, string chatId);
    }
}