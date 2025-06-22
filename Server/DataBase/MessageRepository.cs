using DataBase.Interfaces;
using Models.Messages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DataBase
{
    public class MessageRepository : IMessageRepository
    {
        private const string ConnectionString = "mongodb://localhost:27017";
        private readonly IMongoDatabase _db;

        public MessageRepository()
        {
            var client = new MongoClient(ConnectionString);
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            _db = client.GetDatabase("Education");
        }
        public IEnumerable<MessageModel> GetAllMessages(IndividualMessageFilter filter)
        {
            var result = new List<MessageModel>();
            result.AddRange(_db.GetCollection<MessageModel>("Messages").AsQueryable()
                .Where(x => x.ChatId == filter.ChatId)
                //.Where(x => x.FromName == filter.FromName || x.ToName == filter.ToName)
                .ToList().OrderBy(x => x.SentAt));

            return result;
        }

        public void AddMessage(MessageModel message) 
        {
            _db.CreateCollection("Messages");
            _db.GetCollection<MessageModel>("Messages").InsertOne(message);
        }

        public int GetId()
        {
            return (new Random().Next(3, 600));
        }
    }
}
