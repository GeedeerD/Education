using DataBase.Interfaces;
using fiwe.Models;

namespace fiwe.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ChatViewModel>> GetChatsByUserIdAsync(string userId);
    }
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<ChatViewModel>> GetChatsByUserIdAsync(string userId)
        {
            var chats = await _messageRepository.GetChatsAsync(MongoDB.Bson.ObjectId.Parse(userId));
            return chats.Select(ChatViewModel.WrapModel);
        }
    }
}
