using MongoDB.Bson;

namespace fiwe.Models
{
    public class ViewedModel
    {
        public ObjectId WhoViewed { get; set; }
        public UserModel UserWhoViewed { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
