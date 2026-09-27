using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataBase.Models
{
    public class ImageModelDto
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }
        public ObjectId UploadedByUserObjectId { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Data { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
