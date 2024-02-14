using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Hv.Sos100.SingleSignOn;

internal class ApiService
{
    private const string BaseUrl = "https://informatik6.ei.hv.se/singlesignon";
    private readonly HttpClient _httpClient = new();

    internal async Task<User?> ValidateNewAuthentication(User user)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/Users/validateNewAuthentication", user);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>();
        }

        return null;
    }

    internal async Task<User?> ValidateExistingAuthentication(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/Users/validateExistingAuthentication");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>();
        }

        return null;
    }
}
