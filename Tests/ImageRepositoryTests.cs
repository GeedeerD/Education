using DataBase;
using DataBase.Models;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests
{
    public class ImageRepositoryTests : IDisposable
    {
        private MongoDbRunner _runner;
        private ImageRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("test-db");

            _repo = new ImageRepository(database);
        }

        private static ImageModelDto CreateImage(byte[]? data = null)
        {
            data ??= [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
            return new ImageModelDto
            {
                UploadedByUserObjectId = ObjectId.GenerateNewId(),
                FileName = "test.png",
                ContentType = "image/png",
                Data = data,
                SizeInBytes = data.Length,
                UploadedAt = DateTime.UtcNow
            };
        }

        [Test]
        public async Task SaveImage_ByNewImage_ShouldReturnNewObjectId()
        {
            // Arrange
            var image = CreateImage();

            // Action
            var imageId = await _repo.SaveImageAsync(image);

            // Assert
            Assert.That(imageId, Is.Not.EqualTo(ObjectId.Empty));
            Assert.That(imageId, Is.EqualTo(image.ObjectId));
        }

        [Test]
        public async Task SaveImage_TwoImages_ShouldReturnDifferentObjectIds()
        {
            // Arrange
            var image1 = CreateImage();
            var image2 = CreateImage();

            // Action
            var imageId1 = await _repo.SaveImageAsync(image1);
            var imageId2 = await _repo.SaveImageAsync(image2);

            // Assert
            Assert.That(imageId1, Is.Not.EqualTo(imageId2));
        }

        [Test]
        public async Task GetImage_ById_ShouldReturnSavedImage()
        {
            // Arrange
            var image = CreateImage();
            var imageId = await _repo.SaveImageAsync(image);

            // Action
            var result = await _repo.GetImageByIdAsync(imageId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ObjectId, Is.EqualTo(imageId));
            Assert.That(result.UploadedByUserObjectId, Is.EqualTo(image.UploadedByUserObjectId));
            Assert.That(result.FileName, Is.EqualTo(image.FileName));
            Assert.That(result.ContentType, Is.EqualTo(image.ContentType));
            Assert.That(result.SizeInBytes, Is.EqualTo(image.SizeInBytes));
            Assert.That(result.Data, Is.EqualTo(image.Data));
        }

        [Test]
        public async Task GetImage_ById_ShouldReturnImageDataUnchanged()
        {
            // Arrange — 1 MB случайных байт, чтобы проверить, что бинарные данные не искажаются
            var data = new byte[1024 * 1024];
            new Random(42).NextBytes(data);
            var imageId = await _repo.SaveImageAsync(CreateImage(data));

            // Action
            var result = await _repo.GetImageByIdAsync(imageId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Data, Is.EqualTo(data));
        }

        [Test]
        public async Task GetImage_ByNotExistingId_ShouldReturnNull()
        {
            // Arrange
            await _repo.SaveImageAsync(CreateImage());

            // Action
            var result = await _repo.GetImageByIdAsync(ObjectId.GenerateNewId());

            // Assert
            Assert.That(result, Is.Null);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
