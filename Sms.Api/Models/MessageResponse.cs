namespace Sms.Api.Models;

public record MessageResponse(
    Guid Id,
    string To,
    string Body,
    string Status,
    string? ProviderId,
    string? Error,
    DateTime CreatedAt,
    DateTime UpdatedAt);