using System.Net.Http.Json;
using System.Text.Json;

namespace Sms.Api.Providers;

public record SmsProviderResponse(Guid Id, string Status);

public class HttpSmsProvider(HttpClient client)
{
    public async Task<SmsProviderResponse> SendAsync(
        string to,
        string body)
    {
        using var response = await client.PostAsJsonAsync(
            "sms",
            new { to, body });

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<SmsProviderResponse>();

        return result
            ?? throw new JsonException(
                "The SMS provider returned an empty response.");
    }

    public async Task<SmsProviderResponse> GetStatusAsync(Guid id)
    {
        using var response = await client.GetAsync($"sms/{id}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<SmsProviderResponse>();

        return result
            ?? throw new JsonException(
                "The SMS provider returned an empty response.");
    }
}