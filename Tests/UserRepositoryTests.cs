using DataBase;
using DataBase.Models;
using Mongo2Go;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests
{
    public class UserRepositoryTests
    {
        private MongoDbRunner _runner;
        private MongoClient _client;
        private UserRepository _repo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _runner = MongoDbRunner.Start();
            _client = new MongoClient(_runner.ConnectionString);
        }

        [SetUp]
        public void SetUp()
        {
            // Один mongod на класс, но своя база на каждый тест
            var database = _client.GetDatabase($"test-db-{Guid.NewGuid():N}");
            _repo = new UserRepository(database);
        }

        [Test]
        public async Task CreateAndGetUser_ByUserName_ShouldReturnUser()
        {
            const string userName = "test34@mail.com";
            var user = new UserDto { Email = userName, UserName = userName };

            await _repo.CreateUserAsync(user.UserName, "test", "test-public-name");
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

            user.Id = await _repo.CreateUserAsync(user.UserName, "test", publicUserName);

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

            user.Id = await _repo.CreateUserAsync(user.UserName, "test", "test-public-name");
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

            user.Id = await _repo.CreateUserAsync(user.UserName, "test", "test-public-name");
            await _repo.UpdateUserInfoAsync(user.Id, phoneNumber, null, null);

            // Action
            var result = await _repo.SearchByPhoneAsync(phoneNumber);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.Id, Is.AnyOf(result.Select(x => x.UserId).ToArray()));
        }

        [Test]
        public async Task CreateNewUserAndSetPublicKey_GetUserPublicKey_ShouldReturnUserPublicKeyResult()
        {
            // Arrange
            const string userName = "test34@mail.com";
            string publicKey = Guid.NewGuid().ToString();

            var user = new UserDto { Email = userName, UserName = userName };

            user.Id = await _repo.CreateUserAsync(user.UserName, "test", "test-public-name");
            await _repo.SetUserPublicKeyAsync(user.Id, publicKey);

            // Action
            var result = await _repo.GetUserPublicKeyAsync(user.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(publicKey));
        }



        [Test]
        public async Task CreateNewUserWithoutPublicKey_SetUserPublicKey_ShouldReturnUserWithPublicKeyResult()
        {
            // Arrange
            const string userName = "test34@mail.com";
            string publicKey = Guid.NewGuid().ToString();

            var user = new UserDto { Email = userName, UserName = userName };

            user.Id = await _repo.CreateUserAsync(user.UserName, "test", "test-public-name");
            await _repo.SetUserPublicKeyAsync(user.Id, publicKey);

            // Action
            var result = await _repo.GetByUsernameAsync(userName);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.PublicKey, Is.EqualTo(publicKey));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CreateUser_WithEmptyPublicUserName_ShouldThrow(string? publicUserName)
        {
            Assert.ThrowsAsync(Is.InstanceOf<ArgumentException>(),
                () => _repo.CreateUserAsync("test34@mail.com", "test", publicUserName!));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _runner.Dispose();
        }
    }
}
