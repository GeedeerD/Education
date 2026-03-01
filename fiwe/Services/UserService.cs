using System.Security.Cryptography;
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
            HashPassword(password) == hash;

        private string HashPassword(string password)
        {
            using var sha = SHA3_512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
