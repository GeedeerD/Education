using Configurations;
using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace fiwe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            await Task.CompletedTask;
            return Ok($"OK\t{DateTime.Now:F}");
        }
    }
}
