using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataBase.Models
{
    public class ChatModelDto
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }
        public string Name { get; set; }
        public IEnumerable<ObjectId> UserObjectIds { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public ChatType ChatType { get; set; }
        public DateTime ActiveDate { get; set; }
        public bool IsPrivate { get; set; }
    }

    public enum ChatType : byte
    {
        Dialog = 0,
        Note = 1,
        Room = 2
    }
}