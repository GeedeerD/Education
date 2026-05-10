using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using fiwe.Models;
using fiwe.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace fiwe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IOptions<JwtSettings> _options;
        private readonly IUserService _userService;

        public AuthController(IOptions<JwtSettings> options, IUserService userService)
        {
            _options = options;
            _userService = userService;
        }

        //[AllowAnonymous]
        [Route("Register"), HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterUser model)
        {
            var existing = await _userService.GetByUsernameAsync(model.UserName);
            if (existing != null)
                return BadRequest("User already exists");

            await _userService.CreateUserAsync(model.UserName, model.Password);
            return Ok("User registered");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto dto)
        {
            var user = await _userService.GetByUsernameAsync(dto.UserName);
            if (user == null || !_userService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private object GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _options.Value.Issuer,
                claims: claims,
                expires: expires,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
