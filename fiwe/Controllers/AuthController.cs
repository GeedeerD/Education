using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using fiwe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);

            if (signInResult.Succeeded)
            {
                return Ok(signInResult);
            }

            return BadRequest();
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> LogOut(CancellationToken ct)
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private object GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("shdfgksjfgkaj s sad weafgh33 g dt afad asd 23rte a sd dfg hh6rwhw5w5sfgsfds e4t"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: "fiwe",
                audience: "fiwe",
                claims: claims,
                expires: expires,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Login - userName (nick) + password - Seach account
        // Sign up - userName (nick) + password - Create Account
        // SessionID (Claims)  UserName, Role (Admin, Manager), LastSingIn

        // reset password

    }
}
