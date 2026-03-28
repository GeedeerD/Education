using DataBase;
using DataBase.Models;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests
{
    public class ContactRepositoryTests : IDisposable
    {
        private MongoDbRunner _runner;
        private ContactRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("test-db");

            _repo = new ContactRepository(database);
        }

        [Test]
        public async Task CreateNewContact_ForUser_ShouldReturnNewObjectId()
        {
            // Arrange
            var contact = new ContactDto { ChatId = ObjectId.GenerateNewId(), Name  = "Test" , UserId = ObjectId.GenerateNewId(), ContactListId = ObjectId.GenerateNewId() };

            // Action
            await _repo.CreateContactAsync(contact);

            // Assert
            Assert.That(contact.Id, Is.Not.EqualTo(ObjectId.Empty));
        }


        [Test]
        public async Task GetContact_ForUser_ShouldReturnCreatedContact()
        {
            var userId = ObjectId.GenerateNewId();
            // Arrange
            var contact = new ContactDto { Name = "Test", UserId = userId, ContactListId = ObjectId.GenerateNewId(), ChatId = ObjectId.GenerateNewId() };
            await _repo.CreateContactAsync(contact);

            // Action
            var newContact = await _repo.GetContactByObjectIdAsync(contact.ContactListId, contact.Id);

            // Assert
            Assert.That(contact.Id, Is.Not.EqualTo(ObjectId.Empty));
            Assert.That(newContact.Name, Is.EqualTo(contact.Name));
        }

        [Test]
        public async Task DeleteContact_ByContact_ShouldNotReturnContactAfterDeletion()
        {
            var userId = ObjectId.GenerateNewId();
            // Arrange
            var contact = new ContactDto { Name = "Test", UserId = userId, ContactListId = ObjectId.GenerateNewId(), ChatId = ObjectId.GenerateNewId() };
            await _repo.CreateContactAsync(contact);

            // Action
            await _repo.DeleteContactAsync(contact);

            // Assert
            var contactAfterRemove = await _repo.GetContactByObjectIdAsync(contact.ContactListId, contact.Id);
            Assert.That(contactAfterRemove, Is.Null);
        }

        [Test]
        public async Task UpdateContact_ByContact_ShouldReturnContactWithNewName()
        {
            var userId = ObjectId.GenerateNewId();
            // Arrange
            var contact = new ContactDto { Name = "Test", UserId = userId, ContactListId = ObjectId.GenerateNewId(), ChatId = ObjectId.GenerateNewId() };
            await _repo.CreateContactAsync(contact);
            const string newContactName = "Test - Udated";
            contact.Name = newContactName;

            // Action
            await _repo.UpdateContactAsync(contact);

            // Assert
            var contactAfterUpdate = await _repo.GetContactByObjectIdAsync(contact.ContactListId, contact.Id);
            Assert.That(contactAfterUpdate, Is.Not.Null);
            Assert.That(contactAfterUpdate.Name, Is.EqualTo(newContactName));
        }

        [Test]
        public async Task GetContactsByContactListId_ByContactListId_ShouldReturnContacts()
        {
            var contactListId = ObjectId.GenerateNewId();
            // Arrange
            var contact1 = new ContactDto { Name = "Test1", UserId = ObjectId.GenerateNewId(), ContactListId = contactListId, ChatId = ObjectId.GenerateNewId() };
            var contact2 = new ContactDto { Name = "Test2", UserId = ObjectId.GenerateNewId(), ContactListId = contactListId, ChatId = ObjectId.GenerateNewId() };
            var contact3 = new ContactDto { Name = "Test2", UserId = ObjectId.GenerateNewId(), ContactListId = contactListId, ChatId = ObjectId.GenerateNewId() };
            await _repo.CreateContactAsync(contact1);
            await _repo.CreateContactAsync(contact2);
            await _repo.CreateContactAsync(contact3);

            // Action
            var contacts = await _repo.GetContactsByContactListIdAsync(contactListId);

            // Assert
            Assert.That(contact1.Name, Is.EqualTo(contacts.First(x => x.Id == contact1.Id).Name));
            Assert.That(contact2.Name, Is.EqualTo(contacts.First(x => x.Id == contact2.Id).Name));
            Assert.That(contact3.Name, Is.EqualTo(contacts.First(x => x.Id == contact3.Id).Name));
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
        [Test]
        public async Task CreateChatAsync_WhenExistingChatExists_ShouldUseExistingChat()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId();

            var existingChat = new ChatModelDto
            {
                ObjectId = ObjectId.GenerateNewId(),
                UserObjectIds = new List<ObjectId> { userId },
                ActiveDate = DateTime.UtcNow.AddDays(-1)
            };

            var chats = _database.GetCollection<ChatModelDto>("Chats");

            await chats.InsertOneAsync(existingChat);

            var newChat = new ChatModelDto
            {
                UserObjectIds = new List<ObjectId> { userId }
            };

            // Act
            await _repo.CreateChatAsync(userId, newChat);

            // Assert

            // 1️⃣ проверяем что взялся старый чат
            Assert.That(newChat.ObjectId, Is.EqualTo(existingChat.ObjectId));

            // 2️⃣ проверяем что новый чат НЕ создался
            var allChats = await chats.Find(_ => true).ToListAsync();
            Assert.That(allChats.Count, Is.EqualTo(1));

            // 3️⃣ проверяем что обновилась дата активности
            var updatedChat = allChats.First();
            Assert.That(updatedChat.ActiveDate, Is.GreaterThan(existingChat.ActiveDate));
        }
    }
}
