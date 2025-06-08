using DataBase;
using Microsoft.AspNetCore.Mvc;
using Models.Messages;

namespace fiwe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : Controller
    {
        private readonly MessageRepository _messageRepository;
        public MessagesController()
        {
            _messageRepository = new MessageRepository();
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet(Name = "GetAllMyMessages")]
        public IActionResult Get()
        {
            var from = Request.Headers["x-from"];
            if (from.Any() == false )
            {
                return NotFound();
            }

            var messages = _messageRepository.GetAllMessages(new IndividualMessageFilter() { FromName = from.First(), ToName = from.First() });
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
