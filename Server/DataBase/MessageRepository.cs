using System.Linq;
using DataBase.Interfaces;
using DataBase.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataBase
{
    public class MessageRepository : IMessageRepository
    {
        private const string ConnectionString = "mongodb://localhost:27017";
        private readonly IMongoCollection<MessageModelDto> _messages;
        private readonly IMongoCollection<ChatModelDto> _chats;
        private readonly IMongoDatabase _db;


        public MessageRepository()
        {
            var client = new MongoClient(ConnectionString);

            _db = client.GetDatabase("Education");
            _messages = _db.GetCollection<MessageModelDto>("Messages");
            _chats = _db.GetCollection<ChatModelDto>("Chats");
        }

        public MessageRepository(IMongoDatabase db) : this()
        {
            _db = db;
            _messages = _db.GetCollection<MessageModelDto>("Messages");
            _chats = _db.GetCollection<ChatModelDto>("Chats");
        }

        public async Task CreateChatAsync(ObjectId userId, ChatModelDto chat)
        {
            if (chat.UserObjectIds.Contains(userId))
            {
                var existingChat = await _chats.Find(x => x.UserObjectIds.Contains(userId) && chat.UserObjectIds.Count() == x.UserObjectIds.Count() && x.UserObjectIds.All(o => chat.UserObjectIds.Contains(o))).FirstOrDefaultAsync();
                if (existingChat != null)
                {
                    var update = Builders<ChatModelDto>.Update
                        .Set(u => u.ActiveDate, DateTime.UtcNow);
                    await _chats.FindOneAndUpdateAsync(x => x.ObjectId == existingChat.ObjectId, update);
                    chat.ActiveDate = DateTime.UtcNow;
                    chat.ObjectId = existingChat.ObjectId;
                    return;
                }
                await _chats.InsertOneAsync(chat);
            }
            else
            {
                throw new ArgumentNullException(nameof(chat.UserObjectIds));
            }
        }

        public async Task SendMessageAsync(MessageModelDto message)
        {
            if (message.FromUserObjectId == ObjectId.Empty || message.ChatId == ObjectId.Empty)
            {
                throw new ArgumentException("Need concret FromUserObjectId or ChatId to send message");
            }

            await _messages.InsertOneAsync(message);
            await UpdateChatAvtiveDateAsync(message.ChatId);
        }

        public async Task<MessageModelDto?> ReadMessageAsync(ObjectId userId, ObjectId messageId)
        {

            var message = (await _messages.FindAsync(m => m.ObjectId == messageId))?.FirstOrDefault();
            if(message == null)
            {
                return null;
            }

            var chat = await GetChatAsync(message.ChatId);
            if (chat == null)
            {
                return null;
            }

            if(chat.UserObjectIds.Contains(userId))
            {
                if (message.FromUserObjectId != userId)
                {
                    // mark as read
                }
                return message;
            }

            return null;
        }

        public async Task<IEnumerable<MessageModelDto>> GetMessagesAsync(ObjectId userId, ObjectId chatId, int? messageCount = null)
        {
            var chat = await GetChatAsync(chatId);
            if (!chat.UserObjectIds.Contains(userId))
            {
                return [];
            }

            var messages = await _messages.FindAsync(m => m.ChatId == chatId);
            return messages.ToList();
        }

        public async Task<ChatModelDto> GetChatByUserOjectIdsAsync(ObjectId userId, ObjectId contactUserId)
        {
            var chat = await _chats.Find(x => x.UserObjectIds.Contains(userId) && x.UserObjectIds.Contains(contactUserId) && x.ChatType == ChatType.Dialog).FirstOrDefaultAsync();
            return chat;
        }

        public async Task<ChatModelDto> GetChatAsync(ObjectId chatId)
        {
            var chat = await _chats.Find(x => x.ObjectId == chatId).FirstOrDefaultAsync();
            return chat;
        }

        public async Task<IEnumerable<ChatModelDto>> GetChatsAsync(ObjectId userId)
        {
            var chats = await _chats.Find(x => x.UserObjectIds.Contains(userId)).ToListAsync();
            return chats.OrderByDescending(x => x.ActiveDate);
        }

        private async Task UpdateChatAvtiveDateAsync(ObjectId chatId)
        {
            var update = Builders<ChatModelDto>.Update
                .Set(u => u.ActiveDate, DateTime.UtcNow);
            var chat = await _chats.FindOneAndUpdateAsync(x => x.ObjectId == chatId, update);
        }

        public async Task<AccessInfo> GetChatAccessInfoAsync(string userId, string chatId)
        {
            var chat = await GetChatAsync(ObjectId.Parse(chatId));
            if (chat != null && chat.UserObjectIds.Any(x => x == ObjectId.Parse(userId)))
            {
                return new AccessInfo { Type = AccessType.General, Recipients = chat.UserObjectIds.Where(x => x != ObjectId.Parse(userId)).Select(x => x.ToString()) };
            }

            return new AccessInfo();
        }
    }
}
