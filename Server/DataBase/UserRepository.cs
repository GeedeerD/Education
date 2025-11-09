using DataBase.Interfaces;
using DataBase.Models;
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
        public async Task CreateUserAsync(string username, string passwordHash)
        {
            var user = new UserDto
            {
                UserName = username,
                PasswordHash = passwordHash
            };
            await _users.InsertOneAsync(user);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();
        }
    }
}
