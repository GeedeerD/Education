using MongoDB.Bson;

namespace fiwe.Models
{
    public class MessageModel
    {
        public ObjectId MessageObjectId { get; set; }
        public ObjectId ChatId { get; set; }
        public ChatModel Chat { get; set; }
        public ObjectId FromUserObjectId { get; set; }
        public string MessageBody { get; set; }
        public IEnumerable<ViewedModel> WhoViewed { get; set; } = [];
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }

    public class ChatModel
    {
        public ObjectId ObjectId { get; set; }
        public string Name { get; set; }
        public IEnumerable<ObjectId> UserObjectIds { get; set; } = [];
        public IEnumerable<UserModel> Users { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class MessageViewModel
    {
        public string ChatId { get; set; }
        public string MessageBody { get; set; }
    }
}
