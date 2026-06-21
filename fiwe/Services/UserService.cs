using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DataBase.Interfaces;
using fiwe.Models;
using MongoDB.Bson;

namespace fiwe.Services
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetByUsernameAsync(string username);
        Task CreateUserAsync(string username, string password);
        bool VerifyPassword(string password, string hash);
        Task UpdateUserInfoAsync(string userId, string phone, string email, string publicUserName);
        Task SetUserPublicKeyAsync(string userId, string publicKey);
        Task<string> GetUserPublicKeyAsync(string currentUserId, string userId);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApplicationUser?> GetByUsernameAsync(string username)
        {
            var dto = await _userRepository.GetByUsernameAsync(username);
            if (dto == null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = dto.Id,
                PasswordHash = dto.PasswordHash,
                Username = dto.UserName,
            };
        }

        public async Task CreateUserAsync(string username, string password)
        {
            await _userRepository.CreateUserAsync(username, HashPassword(password));
        }

        public bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task UpdateUserInfoAsync(string userId, string phone, string email, string publicUserName)
        {
            await _userRepository.UpdateUserInfoAsync(ObjectId.Parse(userId), phone, email, publicUserName);
        }

        public async Task SetUserPublicKeyAsync(string userId, string publicKey)
        {
            await _userRepository.SetUserPublicKeyAsync(ObjectId.Parse(userId), publicKey);
        }

        public async Task<string> GetUserPublicKeyAsync(string currentUserId, string userId)
        {
            
            return await _userRepository.GetUserPublicKeyAsync(ObjectId.Parse(userId) != default ? ObjectId.Parse(userId) : ObjectId.Parse(currentUserId));
        }
    }
}
