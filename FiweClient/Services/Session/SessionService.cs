using System.IdentityModel.Tokens.Jwt;

namespace FiweClient.Services.Session;

public interface ISessionService
{
    string? Token { get; }
    string? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }

    void SetSession(string token, string userId, string username);
    void Clear();

    /// <summary>Проверяет не истёк ли JWT токен</summary>
    bool IsTokenValid(string token);
}

public class SessionService : ISessionService
{
    public string? Token { get; private set; }
    public string? UserId { get; private set; }
    public string? Username { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public void SetSession(string token, string userId, string username)
    {
        Token = token;
        UserId = userId;
        Username = username;
    }

    public void Clear()
    {
        Token = null;
        UserId = null;
        Username = null;
    }

    public bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow.AddMinutes(1);
        }
        catch { return false; }
    }
}
