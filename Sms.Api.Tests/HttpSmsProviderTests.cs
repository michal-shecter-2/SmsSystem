using System.Net;
using System.Net.Http.Json;
using Sms.Api.Providers;
using Xunit;

namespace Sms.Api.Tests;

public class HttpSmsProviderTests
{
    [Fact]
    public async Task SendAsync_ReturnsProviderResponse()
    {
        var providerId = Guid.NewGuid();

        using var handler = new TestHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(new
                {
                    id = providerId,
                    status = "accepted"
                })
            });

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };

        var provider = new HttpSmsProvider(client);

        var result = await provider.SendAsync(
            "0501234567",
            "שלום");

        Assert.Equal(providerId, result.Id);
        Assert.Equal("accepted", result.Status);
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenProviderReturnsServerError()
    {
        var calls = 0;

        using var handler = new TestHandler(_ =>
        {
            calls++;

            return new HttpResponseMessage(
                HttpStatusCode.InternalServerError);
        });

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };

        var provider = new HttpSmsProvider(client);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            provider.SendAsync("0501234567", "שלום"));

        Assert.Equal(1, calls);
    }

    private class TestHandler : HttpMessageHandler
    {
        private readonly Func<
            HttpRequestMessage,
            HttpResponseMessage> _respond;

        public TestHandler(
            Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_respond(request));
        }
    }
}