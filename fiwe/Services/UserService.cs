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
        Task CreateUserAsync(string username, string password, string? publicUserName = null);
        bool VerifyPassword(string password, string hash);
        Task UpdateUserInfoAsync(string userId, string phone, string email, string publicUserName);
        Task SetUserPublicKeyAsync(string userId, string publicKey);
        Task<string> GetUserPublicKeyAsync(string currentUserId, string userId);

        /// <summary>Ставит аватар из уже загруженного изображения; imageId == null — убрать аватар.</summary>
        Task SetAvatarAsync(string userId, string? imageId);

        /// <summary>Относительный URL аватара (например "Images/{id}/Download") или null.</summary>
        Task<string?> GetAvatarUrlAsync(string userId);
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

        private const int PublicUserNameLength = 16;

        public async Task CreateUserAsync(string username, string password, string? publicUserName = null)
        {
            if (string.IsNullOrWhiteSpace(publicUserName))
            {
                publicUserName = GenerateRandomString(PublicUserNameLength);
            }

            await _userRepository.CreateUserAsync(username, HashPassword(password), publicUserName);
        }

        private static string GenerateRandomString(int length = 128)
        {
            if (length is < 1 or > 128)
                throw new ArgumentOutOfRangeException(nameof(length), "Длина должна быть от 1 до 128.");

            var sb = new StringBuilder(length + 32);
            while (sb.Length < length)
                sb.Append(Guid.NewGuid().ToString("N"));

            return sb.ToString(0, length);
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

        public async Task SetAvatarAsync(string userId, string? imageId)
        {
            // Храним готовый относительный URL — клиент скачивает его тем же запросом, что и картинки в чате
            var avatarUrl = imageId == null ? null : $"Images/{imageId}/Download";
            await _userRepository.SetAvatarUrlAsync(ObjectId.Parse(userId), avatarUrl);
        }

        public async Task<string?> GetAvatarUrlAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out var objectId) || objectId == ObjectId.Empty)
            {
                return null;
            }

            return await _userRepository.GetAvatarUrlAsync(objectId);
        }
    }
}
