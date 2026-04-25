using fiwe.Models;
using fiwe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MessagesController : BaseApiController
    {
        private const int MessagesCountByChatDueLoading = 5;
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;
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

        [HttpGet, Route("{chatId}/Messages")]
        public async Task<IActionResult> GetMessagesAsync(string chatId)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Unauthorized();
            }

            var messages = await _messageService.GetMessagesFromChatAsync(CurrentUserId, chatId, MessagesCountByChatDueLoading);

            return Ok(messages);
        }


        [HttpPost, Route("SendMessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] MessageViewModel model)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Unauthorized();
            }

            var sendMessageResult = await _messageService.SendMessageAsync(CurrentUserId, model);
            await _hubContext.Clients.Group(model.ChatId).SendAsync("ReceiveMessage", model.ChatId, sendMessageResult.Recipients, sendMessageResult.MessageId);

            return Ok(sendMessageResult.MessageId);
        }
    }
}
