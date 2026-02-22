using MongoDB.Bson;

namespace DataBase.Models
{
    public class IndividualMessageFilter
    {
        public ObjectId? MessageId { get; set; }
        public Guid? ChatId { get; set; }
        public ObjectId? FromUserObjectId { get; set; }
        public ObjectId? ToUserObjectId { get; set; }
        public DateTime? FromDate { get; set; }
    }
}