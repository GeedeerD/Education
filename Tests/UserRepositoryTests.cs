using DataBase;
using DataBase.Models;
using Mongo2Go;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests
{
    public class UserRepositoryTests : IDisposable
    {
        private MongoDbRunner _runner;
        private UserRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("test-db");

            _repo = new UserRepository(database);
        }

        [Test]
        public async Task CreateAndGetUser_ByUserName_ShouldReturnUser()
        {
            const string userName = "test34@mail.com";
            var user = new UserDto { Email = userName, UserName = userName };

            await _repo.CreateUserAsync(user.UserName, "test");
            var result = await _repo.GetByUsernameAsync(userName);

            Assert.That(result, Is.Not.Null);
            Assert.That(userName, Is.EqualTo(result!.UserName));
        }


        [Test]
        public async Task CreateAndSearchUser_ByPublicUserName_ShouldReturnUserSearchResult()
        {
            // Arrange
            const string userName = "test34@mail.com";
            const string publicUserName = "Public UserName";
            var user = new UserDto { Email = userName, UserName = userName, PublicUserName = publicUserName };

            user.Id = await _repo.CreateUserAsync(user.UserName, "test");
            await _repo.UpdateUserInfoAsync(user.Id, null, null, publicUserName);

            // Action
            var result = await _repo.SearchByPublicUserNameAsync(publicUserName);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.Id, Is.AnyOf(result.Select(x => x.UserId).ToArray()));
            Assert.That(publicUserName, Is.AnyOf(result.Select(x => x.PublicUserName).ToArray()));
        }

        [Test]
        public async Task CreateAndSearchUser_ByEmail_ShouldReturnUserSearchResult()
        {
            // Arrange
            const string userName = "test34@mail.com";
            const string email = userName;

            var user = new UserDto { Email = userName, UserName = userName };

            user.Id = await _repo.CreateUserAsync(user.UserName, "test");
            await _repo.UpdateUserInfoAsync(user.Id, null, email, null);

            // Action
            var result = await _repo.SearchByEmailAsync(email);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.Id, Is.AnyOf(result.Select(x => x.UserId).ToArray()));
        }

        [Test]
        public async Task CreateAndSearchUser_ByPhoneNumber_ShouldReturnUserSearchResult()
        {
            // Arrange
            const string userName = "test34@mail.com";
            const string phoneNumber = "+3923028389283";

            var user = new UserDto { Email = userName, UserName = userName };

            user.Id = await _repo.CreateUserAsync(user.UserName, "test");
            await _repo.UpdateUserInfoAsync(user.Id, phoneNumber, null, null);

            // Action
            var result = await _repo.SearchByPhoneAsync(phoneNumber);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.Id, Is.AnyOf(result.Select(x => x.UserId).ToArray()));
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
