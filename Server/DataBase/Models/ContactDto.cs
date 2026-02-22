using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataBase.Models
{
    public class ContactDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId ContactListId { get; set; }
        public ObjectId ChatId { get; set; }
        public ObjectId UserId { get; set; }// invisible for UI
        public string Name { get; set; }
        //public string UserName { get; set; }
        //public ContactStatus Status { get; set; }
        //public DateTime? LastOnlineDate { get; set; }
        //public bool IsOnline { get; set; }
        public bool IsDeleted { get; set; }
        //public string AvatarUrl { get; set; }
    }

    public enum ContactStatus : byte
    {
        Unknown = 0,
        Busy = 1,
        Invisible = 2,

    }
}