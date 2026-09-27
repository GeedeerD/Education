using fiwe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ImagesController : BaseApiController
    {
        private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB

        // Небольшой запас поверх лимита, чтобы запрос дошёл до нашей проверки
        // и мы могли вернуть осмысленный 403, а не общий 413 от Kestrel.
        private const long RequestSizeLimitBytes = MaxImageSizeBytes + 1024 * 1024;

        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost, Route("Upload")]
        [RequestSizeLimit(RequestSizeLimitBytes)]
        public async Task<IActionResult> UploadImageAsync(IFormFile file)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            if (file.Length > MaxImageSizeBytes)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "File size exceeds the 10 MB limit.");
            }

            var imageId = await _imageService.UploadImageAsync(CurrentUserId, file);
            return Ok(new { ImageId = imageId });
        }

        [HttpGet, Route("{imageId}/Download")]
        public async Task<IActionResult> DownloadImageAsync(string imageId)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Unauthorized();
            }

            var image = await _imageService.GetImageAsync(imageId);
            if (image == null)
            {
                return NotFound();
            }

            return File(image.Data, image.ContentType, image.FileName);
        }
    }
}
