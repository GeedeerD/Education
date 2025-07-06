using fiwe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace fiwe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //key


        //password
        //userName
        [HttpPost(Name = "Register")]
        public async Task<IActionResult> RegisterAsync(RegisterUser model)
        {

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Password = model.Password,
            };
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                return Ok("User geristerd");
            }

            return BadRequest();
        }

        // Login - userName (nick) + password - Seach account
        // Sign up - userName (nick) + password - Create Account
                // SessionID (Claims)  UserName, Role (Admin, Manager), LastSingIn

        // reset password

    }
}
