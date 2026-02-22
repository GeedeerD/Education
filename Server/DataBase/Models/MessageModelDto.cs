using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataBase.Models
{
    public class ViewedModelDto
    {
        public ObjectId WhoViewed { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    public class MessageModelDto
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }
        public ObjectId ChatId { get; set; }
        public ObjectId FromUserObjectId { get; set; }
        public required string MessageBody { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
