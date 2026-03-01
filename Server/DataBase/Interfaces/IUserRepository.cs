using DataBase.Models;
using MongoDB.Bson;

namespace DataBase.Interfaces
{
    public interface IUserRepository
    {
        Task<ObjectId> CreateUserAsync(string username, string passwordHash);
        Task UpdateUserInfoAsync(ObjectId userId, string phoneNumber, string email, string publicUserName);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<IEnumerable<UserSearchResultDto>> SearchByPublicUserNameAsync(string publicUserName);
        Task<IEnumerable<UserSearchResultDto>> SearchByPhoneAsync(string phone);
        Task<IEnumerable<UserSearchResultDto>> SearchByEmailAsync(string email);
        Task<ObjectId> GetContactListIdAsync(ObjectId userId);
    }
}
