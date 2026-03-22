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
        
        public async Task CreateContact_ShouldSaveContactToDatabase()
        {
            // Arrange
            var contact = new ContactDto
            {
                Name = "Test",
                UserId = ObjectId.GenerateNewId(),
                ContactListId = ObjectId.GenerateNewId(),
                ChatId = ObjectId.GenerateNewId()
            };

            // Action
            await _repo.CreateContactAsync(contact);

            // Assert
            var savedContact = await _repo.GetContactByObjectIdAsync(contact.ContactListId, contact.Id);
            Assert.That(savedContact, Is.Not.Null);
            Assert.That(savedContact.Name, Is.EqualTo(contact.Name));
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
        public async Task CreateContactAsync_WhenContactAlreadyExists_ShouldEnterExistingContact()
        {
            // Arrange
            var contact = new ContactDto
            {
                Name = "Test",
                UserId = ObjectId.GenerateNewId(),
                ContactListId = ObjectId.GenerateNewId(),
                ChatId = ObjectId.GenerateNewId()
            };
            await _repo.CreateContactAsync(contact);
            var duplicate = new ContactDto { Name = "New Name", UserId = contact.UserId, ContactListId = contact.ContactListId, ChatId = contact.ChatId };

            // Act
            await _repo.CreateContactAsync(duplicate);

            // Assert
            Assert.That(duplicate.Id, Is.EqualTo(contact.Id));
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
    }
}
