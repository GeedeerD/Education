using System.Net.Http.Headers;
using System.Net.Http.Json;
using FiweClient.Services.Session;

namespace FiweClient.Services.Api;

/// <summary>
/// Базовый класс для всех API сервисов.
/// Автоматически добавляет JWT токен в заголовок каждого запроса.
/// </summary>
public abstract class BaseApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly ISessionService _session;

    protected BaseApiService(IHttpClientFactory factory, ISessionService session)
    {
        _factory = factory;
        _session = session;
    }

    protected HttpClient CreateClient()
    {
        var client = _factory.CreateClient("FiweApi");

        if (_session.Token is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.Token);
        }

        return client;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        if (typeof(string) == typeof(T))
        {
            var str = await response.Content.ReadAsStringAsync();
            return (T)(object)str;
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<T?> PostAsync<T>(string url, object body)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        if(typeof(string) == typeof(T))
        {
            var str = await response.Content.ReadAsStringAsync();
            return (T)(object)str; 
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task PostAsync(string url, object body)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<T?> PostFileAsync<T>(string url, byte[] fileBytes, string fileName, string contentType)
    {
        var client = CreateClient();
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<byte[]> GetBytesAsync(string url)
    {
        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
