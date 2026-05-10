using FiweClient.Services.Session;

namespace FiweClient.Services.Api;

public interface IAuthApiService
{
    Task<LoginResponse?> LoginAsync(string username, string password);
    Task RegisterAsync(string username, string password);
}

public class AuthApiService : BaseApiService, IAuthApiService
{
    public AuthApiService(IHttpClientFactory factory, ISessionService session)
        : base(factory, session) { }

    public Task<LoginResponse?> LoginAsync(string username, string password)
        => PostAsync<LoginResponse>("Auth/login", new LoginRequest(username, password));

    public Task RegisterAsync(string username, string password)
        => PostAsync("Auth/Register", new RegisterRequest(username, password));
}
