using DataBase.Models;
using MongoDB.Bson;

namespace DataBase.Interfaces
{
    public interface IImageRepository
    {
        Task<ObjectId> SaveImageAsync(ImageModelDto image);
        Task<ImageModelDto?> GetImageByIdAsync(ObjectId imageId);
    }
}
