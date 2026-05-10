using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataBase.Models
{

    public class UserDto
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; set; } = null!;
        public string PublicUserName { get; set; } = null!;
        public string PublicKey { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = null!;
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public byte AccessFailedCount { get; set; }
        public ObjectId ContactListId { get; set; }
        public IEnumerable<string> AvatarUrls { get; set; }
    }

    public class UserSearchResultDto
    {
        public ObjectId UserId { get; set; }
        public required string PublicUserName { get; set; }
        public string AvatarUrl { get; set; }
        public static UserSearchResultDto WrapModel(UserDto user)
        {
            return new UserSearchResultDto { UserId = user.Id, PublicUserName = user.PublicUserName, AvatarUrl = user.AvatarUrls?.LastOrDefault() };
        }
    }
}