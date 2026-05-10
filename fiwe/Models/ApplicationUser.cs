using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fiwe.Models
{
    public class ApplicationUser
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;
    }
}