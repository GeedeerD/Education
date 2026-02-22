using fiwe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MessagesController : BaseApiController
    {
        private readonly IMessageService _messageService;
        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
            
        }

        [HttpGet, Route("GetAllMyChats")]
        public async Task<IActionResult> GetAllMyChats()
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Unauthorized();
            }

            var chats = await _messageService.GetChatsByUserIdAsync(CurrentUserId);
            
            return Ok(chats);
        }
    }
}
