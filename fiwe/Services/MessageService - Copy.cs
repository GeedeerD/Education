using DataBase.Interfaces;
using DataBase.Models;
using fiwe.Models;
using MongoDB.Bson;

namespace fiwe.Services
{
    public interface IContactService
    {
        Task<string> AddContatAsync(string contactListId, string userId);
        Task<IEnumerable<UserViewModel>> FindUsersByUserNameAsync(string userName);
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

        public ContactService(IContactRepository contactRepository, IUserRepository userRepository)
        {
            _contactRepository = contactRepository;
            _userRepository = userRepository;
        }

        public async Task<string> AddContatAsync(string contactListId, string userId)
        {
            var contactDto = new ContactDto { ContactListId = ObjectId.Parse(contactListId), UserId = ObjectId.Parse(userId) };
            await _contactRepository.CreateContactAsync(contactDto);
            return contactDto.Id.ToString();
        }

        public async Task<IEnumerable<UserViewModel>> FindUsersByUserNameAsync(string userName)
        {
            var userDtos = await _userRepository.SearchByPublicUserNameAsync(userName);
            return userDtos.Select(UserViewModel.WrapModel);
        }
    }
}
