using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected string CurrentUserId => User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
