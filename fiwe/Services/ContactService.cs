using DataBase.Interfaces;
using DataBase.Models;
using MongoDB.Bson;

namespace fiwe.Services
{
    public interface IContactService
    {
        Task<string> AddContatAsync(string currentUserId, string userId, string contactName);
        Task<IEnumerable<UserViewModel>> FindUsersByUserNameAsync(string publicUserName);
    }

    public class UserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public static UserViewModel WrapModel(UserSearchResultDto user)
        {
            return new UserViewModel { UserId = user.UserId.ToString(), UserName = user.PublicUserName };
        }
    }

    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;

        public ContactService(IContactRepository contactRepository, IUserRepository userRepository, IMessageRepository messageRepository)
        {
            _contactRepository = contactRepository;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        public async Task<string> AddContatAsync(string currentUserId, string userId, string contactName)
        {
            var userContactListId = await _userRepository.GetContactListIdAsync(ObjectId.Parse(currentUserId));
            var chatDto = new ChatModelDto() { UserObjectIds = new HashSet<ObjectId>() { ObjectId.Parse(currentUserId), ObjectId.Parse(userId) } };
            await _messageRepository.CreateChatAsync(ObjectId.Parse(currentUserId), chatDto);
            
            var contactDto = new ContactDto { ContactListId = userContactListId, UserId = ObjectId.Parse(userId), ChatId = chatDto.ObjectId, Name = contactName };
            await _contactRepository.CreateContactAsync(contactDto);
            return contactDto.Id.ToString();
        }

        public async Task<IEnumerable<UserViewModel>> FindUsersByUserNameAsync(string publicUserName)
        {
            var userDtos = await _userRepository.SearchByPublicUserNameAsync(publicUserName);
            return userDtos.Select(UserViewModel.WrapModel);
        }
    }
}
