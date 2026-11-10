using System.Text.Json;
using Sms.Api.Data;
using Sms.Api.Models;
using Sms.Api.Providers;

namespace Sms.Api.Services;

public record SendMessageResult(Guid Id, MessageStatus Status);

public class MessageService
{
    private readonly MessageRepository _repository;
    private readonly HttpSmsProvider _provider;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        MessageRepository repository,
        HttpSmsProvider provider,
        ILogger<MessageService> logger)
    {
        _repository = repository;
        _provider = provider;
        _logger = logger;
    }

    public async Task<SendMessageResult> SendAsync(
        SendMessageRequest request)
    {
        var id = await _repository.CreateAsync(
            request.To,
            request.Body);

        var status = MessageStatus.Unknown;
        string? providerId = null;
        string? error = null;

        try
        {
            var response = await _provider.SendAsync(
                request.To,
                request.Body);

            if (response.Id == Guid.Empty)
            {
                throw new JsonException(
                    "Provider returned an invalid ID.");
            }

            status = response.Status switch
            {
                "accepted" => MessageStatus.Accepted,
                "delivered" => MessageStatus.Delivered,
                "failed" => MessageStatus.Failed,
                _ => throw new JsonException(
                    "Provider returned an invalid status.")
            };

            providerId = response.Id.ToString();
        }
        catch (Exception ex) when (
            ex is HttpRequestException
                or TaskCanceledException
                or JsonException)
        {
            error = "provider_outcome_unknown";

            _logger.LogWarning(
                "Provider outcome unknown for message {MessageId}",
                id);
        }

        await _repository.UpdateAsync(
            id,
            status,
            providerId,
            error);

        return new SendMessageResult(id, status);
    }

    public async Task<MessageResponse?> GetAsync(Guid id)
    {
        var message = await _repository.GetAsync(id);

        if (message is null || message.Status != "accepted")
        {
            return message;
        }

        if (!Guid.TryParse(message.ProviderId, out var providerId))
        {
            return message;
        }

        var status = MessageStatus.Accepted;
        string? error = null;

        try
        {
            var response = await _provider.GetStatusAsync(providerId);

            if (response.Id != providerId)
            {
                throw new JsonException(
                    "Provider returned a different message ID.");
            }

            status = response.Status switch
            {
                "accepted" => MessageStatus.Accepted,
                "delivered" => MessageStatus.Delivered,
                "failed" => MessageStatus.Failed,
                _ => throw new JsonException(
                    "Provider returned an invalid status.")
            };
        }
        catch (Exception ex) when (
            ex is HttpRequestException
                or TaskCanceledException
                or JsonException)
        {
            error = "status_refresh_failed";

            _logger.LogWarning(
                "Status refresh failed for message {MessageId}",
                id);
        }

        await _repository.UpdateAsync(
            id,
            status,
            error: error);

        return await _repository.GetAsync(id);
    }

    public Task<List<MessageResponse>> ListAsync(MessageStatus? status)
    {
        return _repository.ListAsync(status);
    }
}