using DataBase.Interfaces;
using DataBase.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataBase
{
    public class UserRepository : IUserRepository
    {
        private readonly string ConnectionString = "mongodb://localhost:27017";
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<UserDto> _users;

        public UserRepository()
        {
            var client = new MongoClient(ConnectionString);

            _db = client.GetDatabase("Education");
            _users = _db.GetCollection<UserDto>("Users");
        }

        public UserRepository(IMongoDatabase db) : this()
        {
            _db = db;
            _users = _db.GetCollection<UserDto>("Users");
        }
        public async Task<ObjectId> CreateUserAsync(string username, string passwordHash)
        {
            var user = new UserDto
            {
                UserName = username,
                PasswordHash = passwordHash,
                ContactListId = ObjectId.GenerateNewId(),
            };
            await _users.InsertOneAsync(user);
            return user.Id;
        }

        public async Task UpdateUserInfoAsync(ObjectId userId, string phoneNumber, string email, string publicUserName)
        {
            var update = Builders<UserDto>.Update.Combine();
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                update = update.Set(u => u.PhoneNumber, phoneNumber);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                update = update.Set(u => u.Email, email);
            }

            if (!string.IsNullOrWhiteSpace(publicUserName))
            {
                update = update.Set(u => u.PublicUserName, publicUserName);
            }

            var user = await _users.FindOneAndUpdateAsync(x => x.Id == userId, update);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserSearchResultDto>> SearchByPublicUserNameAsync(string publicUserName)
        {
            if (string.IsNullOrWhiteSpace(publicUserName))
            {
                return Enumerable.Empty<UserSearchResultDto>();
            }
            var users = await _users.Find(u => u.PublicUserName == publicUserName).ToListAsync();
            return users.Select(UserSearchResultDto.WrapModel);
        }

        public async Task<IEnumerable<UserSearchResultDto>> SearchByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return Enumerable.Empty<UserSearchResultDto>();
            }

            var users = await _users.Find(u => u.PhoneNumber == phone).ToListAsync();
            return users.Select(UserSearchResultDto.WrapModel);
        }

        public async Task<IEnumerable<UserSearchResultDto>> SearchByEmailAsync(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return Enumerable.Empty<UserSearchResultDto>();
            }


            var users = await _users.Find(u => u.Email == email).ToListAsync();
            return users.Select(UserSearchResultDto.WrapModel);
        }
    }
}
