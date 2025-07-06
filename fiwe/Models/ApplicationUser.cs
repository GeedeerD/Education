using AspNetCore.Identity.Mongo.Model;

namespace fiwe.Models
{
    public class ApplicationUser : MongoUser
    {
        //public string UserName { get; set; }
        public required string Password { get; set; }
    }
}