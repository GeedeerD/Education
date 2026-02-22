using MongoDB.Bson;

namespace fiwe.Models
{
    public class UserModel
    {
        public ObjectId ObjectId { get; set; }
        public string UserName { get; set; }
    }
}