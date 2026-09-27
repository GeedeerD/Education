using Configurations;
using DataBase.Interfaces;
using DataBase.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataBase
{
    public class ImageRepository : IImageRepository
    {
        private readonly IMongoCollection<ImageModelDto> _images;

        public ImageRepository(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var db = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _images = db.GetCollection<ImageModelDto>("Images");
        }

        public async Task<ObjectId> SaveImageAsync(ImageModelDto image)
        {
            await _images.InsertOneAsync(image);
            return image.ObjectId;
        }

        public async Task<ImageModelDto?> GetImageByIdAsync(ObjectId imageId)
        {
            return await _images.Find(x => x.ObjectId == imageId).FirstOrDefaultAsync();
        }
    }
}
