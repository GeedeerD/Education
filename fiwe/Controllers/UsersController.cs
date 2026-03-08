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
        public UsersController(IUserService userService)
        {
            _userService = userService;
            
        }

        [HttpPost, Route("ChangePublicName")]
        public async Task<IActionResult> ChangePublicUserAsync([FromBody]string publicUserName)
        {
            await _userService.UpdateUserInfoAsync(CurrentUserId, phone: null, email: null, publicUserName: publicUserName);

            return Ok();
        }
    }
}
