using fiwe.Models;
using fiwe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public partial class ContactsController : BaseApiController
    {
        private readonly IContactService _contactService;
        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
            
        }

        [HttpPost, Route("FindUsers")]
        public async Task<IActionResult> FindUsersAsync([FromBody]string publicUserName)
        {
            var users = await _contactService.FindUsersByUserNameAsync(publicUserName);
            
            return Ok(users);
        }

        [HttpPost, Route("AddContact")]
        public async Task<IActionResult> AddContactAsync(AddContactModel model)
        {
            var newContactId = await _contactService.AddContatAsync(CurrentUserId, model.UserId, model.ContactName);

            return Ok(newContactId);
        }

        /// <summary>Удалить контакт по id его диалога: диалог пропадает из списка только у вызывающего.</summary>
        [HttpPost, Route("DeleteContact")]
        public async Task<IActionResult> DeleteContactAsync([FromBody] string chatId)
        {
            var deleted = await _contactService.DeleteContactByChatAsync(CurrentUserId, chatId);
            return deleted ? Ok() : NotFound();
        }
    }
}
