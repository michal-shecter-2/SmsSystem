using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    [Theory]
    [InlineData("null")]
    [InlineData("not-json")]
    public async Task SendAsync_ThrowsWhenResponseIsInvalid(string body)
    {
        using var handler = new TestHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };
        var provider = new HttpSmsProvider(client);

        await Assert.ThrowsAnyAsync<JsonException>(() =>
            provider.SendAsync("0501234567", "Hello"));
    }

    [Fact]
    public async Task SendAsync_PropagatesTimeoutWithoutRetry()
    {
        var calls = 0;
        using var handler = new TestHandler(_ =>
        {
            calls++;
            throw new TaskCanceledException("Simulated timeout");
        });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };
        var provider = new HttpSmsProvider(client);

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            provider.SendAsync("0501234567", "Hello"));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetStatusAsync_UsesProviderIdAndReturnsStatus()
    {
        var id = Guid.NewGuid();
        using var handler = new TestHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/sms/{id}", request.RequestUri!.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { id, status = "delivered" })
            };
        });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };
        var provider = new HttpSmsProvider(client);

        var result = await provider.GetStatusAsync(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("delivered", result.Status);
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