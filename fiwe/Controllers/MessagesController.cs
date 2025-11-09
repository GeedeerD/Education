using DataBase.Interfaces;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Messages;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [HttpGet, Route("GetAllMyMessages")]
        public IActionResult GetAllMyMessages()
        {
            var currentUser = User.Identity?.Name;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var messages = _messageRepository.GetAllMessages(new IndividualMessageFilter() { FromName = currentUser, ToName = currentUser });
            return Ok(messages);
        }


        [HttpPut(Name = "SendMessage")]
        public IActionResult SendMessage([FromBody] MessageViewModel viewModel)
        {
            var from = Request.Headers["x-from"];
            if (from.Any() == false)
            {
                return NotFound();
            }


            var model = new MessageModel
            {
                Id = GenerateNewId(),
                FromName = from.First(),
                ToName = viewModel.ToName,
                MessageBody = viewModel.MessageBody
            };

            _messageRepository.AddMessage(model);
            return Ok();
        }

        private int GenerateNewId()
        {
            return _messageRepository.GetId();
        }
    }
}
