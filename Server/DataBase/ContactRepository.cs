using Configurations;
using DataBase.Interfaces;
using DataBase.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataBase
{
    public class ContactRepository : IContactRepository
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<ContactDto> _contacts;

        public ContactRepository(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);

            _db = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _contacts = _db.GetCollection<ContactDto>("Contacts");
        }

        public ContactRepository(IMongoDatabase db)
        {
            _db = db;
            _contacts = _db.GetCollection<ContactDto>("Contacts");
        }

        public async Task CreateContactAsync(ContactDto contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (contact.ContactListId == ObjectId.Empty)
            {
                throw new ArgumentNullException(nameof(contact.ContactListId));
            }

            if (contact.UserId == ObjectId.Empty)
            {
                throw new ArgumentNullException(nameof(contact.UserId));
            }

            if (contact.ChatId == ObjectId.Empty)
            {
                throw new ArgumentNullException(nameof(contact.ChatId));
            }

            var existingContact = await _contacts.Find(x=>x.UserId == contact.UserId && x.ContactListId == contact.ContactListId && x.ChatId == contact.ChatId).FirstOrDefaultAsync();
            if (existingContact != null)
            {
                var update = Builders<ContactDto>.Update
                        .Set(u => u.Name, contact.Name);
                await _contacts.FindOneAndUpdateAsync(x => x.Id == existingContact.Id, update);
                contact.Id = existingContact.Id;
                return;
            }

            await _contacts.InsertOneAsync(contact);
        }

        public async Task DeleteContactAsync(ContactDto contact)
        {
            var delete = Builders<ContactDto>.Update
                .Set(u => u.IsDeleted, true);
            await _contacts.FindOneAndUpdateAsync(x => x.Id == contact.Id && x.ContactListId == contact.ContactListId, delete);
        }

        public async Task<IEnumerable<ContactDto>> GetContactsByContactListIdAsync(ObjectId contactListId)
        {
            var contacts = await _contacts.FindAsync(x => x.ContactListId == contactListId && x.IsDeleted == false);
            return await contacts.ToListAsync();
        }

        public async Task<ContactDto> GetContactByObjectIdAsync(ObjectId contactListId, ObjectId contactId)
        {
            var contacts = await _contacts.FindAsync(x => x.ContactListId == contactListId && x.IsDeleted == false && x.Id == contactId);
            return await contacts.FirstOrDefaultAsync();
        }

        public async Task UpdateContactAsync(ContactDto contact)
        {
            var update = Builders<ContactDto>.Update
                .Set(u => u.Name, contact.Name);
                //.Set(u => u.AvatarUrl, contact.AvatarUrl);
            var contactDb = await _contacts.FindOneAndUpdateAsync(x => x.Id == contact.Id && x.ContactListId == contact.ContactListId, update);
        }
    }
}
