﻿using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Hv.Sos100.SingleSignOn;

internal class ApiService
{
    private const string BaseUrl = "https://informatik6.ei.hv.se/singlesignon";
    private readonly HttpClient _httpClient = new();

    internal async Task<Authentication?> ValidateNewAuthentication(Account account)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/Authentications/validateNewSession", account);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Authentication>();
        }

        return null;
    }

    internal async Task<Authentication?> ValidateExistingAuthentication(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/Authentications/validateExistingSession");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Authentication>();
        }

        return null;
    }
}
