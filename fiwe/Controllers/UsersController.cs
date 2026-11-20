using fiwe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public partial class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IImageService _imageService;

        public UsersController(IUserService userService, IImageService imageService)
        {
            _userService = userService;
            _imageService = imageService;
        }

        /// <summary>
        /// Поставить аватар. Картинку сначала загружают через Images/Upload, сюда передают её imageId.
        /// Можно использовать только своё изображение — чужое по imageId поставить нельзя.
        /// </summary>
        [HttpPost, Route("SetAvatar")]
        public async Task<IActionResult> SetAvatarAsync([FromBody] string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId))
            {
                return BadRequest("imageId is required.");
            }

            if (!await _imageService.IsUploadedByAsync(imageId, CurrentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Image not found or uploaded by another user.");
            }

            await _userService.SetAvatarAsync(CurrentUserId, imageId);
            return Ok(new { AvatarUrl = await _userService.GetAvatarUrlAsync(CurrentUserId) });
        }

        [HttpPost, Route("RemoveAvatar")]
        public async Task<IActionResult> RemoveAvatarAsync()
        {
            await _userService.SetAvatarAsync(CurrentUserId, null);
            return Ok();
        }

        /// <summary>Аватар любого пользователя (в т.ч. свой — по своему userId). AvatarUrl == null — аватара нет.</summary>
        [HttpGet, Route("{userId}/Avatar")]
        public async Task<IActionResult> GetAvatarAsync(string userId)
        {
            return Ok(new { AvatarUrl = await _userService.GetAvatarUrlAsync(userId) });
        }

        [HttpPost, Route("ChangePublicName")]
        public async Task<IActionResult> ChangePublicUserAsync([FromBody]string publicUserName)
        {
            await _userService.UpdateUserInfoAsync(CurrentUserId, phone: null, email: null, publicUserName: publicUserName);

            return Ok();
        }

        [HttpPost, Route("SetPublicKey")]
        public async Task<IActionResult> SetUserPublicKeyAsync([FromBody] string publicKey)
        {
            await _userService.SetUserPublicKeyAsync(CurrentUserId, publicKey);

            return Ok();
        }

        [HttpGet, Route("{userId}/PublicKey")]
        public async Task<IActionResult> GetUserPublicKeyAsync(string userId)
        {
            var publicKey = await _userService.GetUserPublicKeyAsync(CurrentUserId, userId);

            return Ok(new { PublicKeyBase64 = publicKey });
        }
    }
}
