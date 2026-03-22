using DataBase.Models;
using MongoDB.Bson;

namespace DataBase.Interfaces
{
    public interface IContactRepository
    {
        Task CreateContactAsync(ContactDto contact);
        Task UpdateContactAsync(ContactDto contact);
        Task DeleteContactAsync(ContactDto contact);
        Task<ContactDto> GetContactByObjectIdAsync(ObjectId contactListId, ObjectId contactId);
        Task<IEnumerable<ContactDto>> GetContactsByContactListIdAsync(ObjectId contactListId);
    }
}
