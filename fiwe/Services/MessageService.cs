using DataBase.Interfaces;
using DataBase.Models;
using fiwe.Models;
using fiwe.Models.Exceptions;
using MongoDB.Bson;

namespace fiwe.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ChatViewModel>> GetChatsByUserIdAsync(string userId);
        Task<IEnumerable<PreviewMessageViewModel>> GetMessagesFromChatAsync(string currentUserId, string chatId, int messageCount);
        Task<string> SendMessageAsync(string userId, MessageViewModel model);
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
            var chats = await _messageRepository.GetChatsAsync(ObjectId.Parse(userId));
            return chats.Select(ChatViewModel.WrapModel);
        }

        public async Task<IEnumerable<PreviewMessageViewModel>> GetMessagesFromChatAsync(string currentUserId, string chatId, int messageCount)
        {
            var previewMessages = await _messageRepository.GetMessagesAsync(ObjectId.Parse(currentUserId), ObjectId.Parse(chatId), messageCount);

            return previewMessages.Select(x => new PreviewMessageViewModel { MessageBody = x.MessageBody, SentAt = x.SentAt, SenderObjectId = x.FromUserObjectId.ToString() });
        }

        public async Task<string> SendMessageAsync(string userId, MessageViewModel model)
        {
            var accessInfo = await _messageRepository.GetChatAccessInfoAsync(userId, model.ChatId);
            if (accessInfo.IsDeny)
            {
                throw new ForbiddenException("User does not have access to the chat.");
            }

            var messageDto = new MessageModelDto() { MessageBody = model.MessageBody, ChatId = ObjectId.Parse(model.ChatId), FromUserObjectId = ObjectId.Parse(userId), SentAt = DateTime.UtcNow };
            await _messageRepository.SendMessageAsync(messageDto);
            return messageDto.ObjectId.ToString();
        }
    }
}
