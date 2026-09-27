using DataBase.Interfaces;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace fiwe.Services
{
    public record ImageDownloadResult(byte[] Data, string ContentType, string FileName);

    public interface IImageService
    {
        Task<string> UploadImageAsync(string userId, IFormFile file);
        Task<ImageDownloadResult?> GetImageAsync(string imageId);
    }

    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<string> UploadImageAsync(string userId, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var image = new ImageModelDto
            {
                UploadedByUserObjectId = ObjectId.Parse(userId),
                FileName = file.FileName,
                ContentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType,
                Data = memoryStream.ToArray(),
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            var imageId = await _imageRepository.SaveImageAsync(image);
            return imageId.ToString();
        }

        public async Task<ImageDownloadResult?> GetImageAsync(string imageId)
        {
            if (!ObjectId.TryParse(imageId, out var objectId))
            {
                return null;
            }

            var image = await _imageRepository.GetImageByIdAsync(objectId);
            if (image == null)
            {
                return null;
            }

            return new ImageDownloadResult(image.Data, image.ContentType, image.FileName);
        }
    }
}
