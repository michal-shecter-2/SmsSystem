using Microsoft.Data.SqlClient;
using Sms.Api.Models;
using System.Data;

namespace Sms.Api.Data;

public class MessageRepository
{
    private readonly string _connectionString;

    public MessageRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("SmsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'SmsDb' is missing.");
    }

    public async Task<Guid> CreateAsync(string to, string body)
    {
        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO dbo.Messages
                (Id, [To], Body, Status)
            VALUES
                (@Id, @To, @Body, @Status);
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);

        command.Parameters
            .Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        command.Parameters
            .Add("@To", SqlDbType.VarChar, 16).Value = to;

        command.Parameters
            .Add("@Body", SqlDbType.NVarChar, 160).Value = body;

        command.Parameters
            .Add("@Status", SqlDbType.VarChar, 16).Value =
                MessageStatus.Pending
                    .ToString()
                    .ToLowerInvariant();

        await command.ExecuteNonQueryAsync();

        return id;
    }

    public async Task UpdateAsync(
    Guid id,
    MessageStatus status,
    string? providerId = null,
    string? error = null)
    {
        const string sql = """
        UPDATE dbo.Messages
        SET
            Status = @Status,
            ProviderId = COALESCE(@ProviderId, ProviderId),
            Error = @Error,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @Id
        AND Status NOT IN ('delivered', 'failed');
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);

        command.Parameters
            .Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        command.Parameters
            .Add("@Status", SqlDbType.VarChar, 16).Value =
                status.ToString().ToLowerInvariant();

        command.Parameters
            .Add("@ProviderId", SqlDbType.NVarChar, 200).Value =
                (object?)providerId ?? DBNull.Value;

        command.Parameters
            .Add("@Error", SqlDbType.VarChar, 100).Value =
                (object?)error ?? DBNull.Value;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<MessageResponse?> GetAsync(Guid id)
    {
        const string sql = """
        SELECT
            Id,
            [To],
            Body,
            Status,
            ProviderId,
            Error,
            CreatedAt,
            UpdatedAt
        FROM dbo.Messages
        WHERE Id = @Id;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);

        command.Parameters
            .Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new MessageResponse(
            Id: reader.GetGuid(0),
            To: reader.GetString(1),
            Body: reader.GetString(2),
            Status: reader.GetString(3),
            ProviderId: reader.IsDBNull(4) ? null : reader.GetString(4),
            Error: reader.IsDBNull(5) ? null : reader.GetString(5),
            CreatedAt: DateTime.SpecifyKind(
                reader.GetDateTime(6), DateTimeKind.Utc),
            UpdatedAt: DateTime.SpecifyKind(
                reader.GetDateTime(7), DateTimeKind.Utc));
    }

    public async Task<List<MessageResponse>> ListAsync(
    MessageStatus? status)
    {
        const string sql = """
        SELECT
            Id, [To], Body, Status,
            ProviderId, Error, CreatedAt, UpdatedAt
        FROM dbo.Messages
        WHERE @Status IS NULL OR Status = @Status
        ORDER BY CreatedAt DESC, Id DESC;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);

        command.Parameters
            .Add("@Status", SqlDbType.VarChar, 16).Value =
                (object?)status?.ToString().ToLowerInvariant()
                ?? DBNull.Value;

        await using var reader = await command.ExecuteReaderAsync();

        var messages = new List<MessageResponse>();

        while (await reader.ReadAsync())
        {
            messages.Add(new MessageResponse(
                Id: reader.GetGuid(0),
                To: reader.GetString(1),
                Body: reader.GetString(2),
                Status: reader.GetString(3),
                ProviderId: reader.IsDBNull(4) ? null : reader.GetString(4),
                Error: reader.IsDBNull(5) ? null : reader.GetString(5),
                CreatedAt: DateTime.SpecifyKind(
                    reader.GetDateTime(6), DateTimeKind.Utc),
                UpdatedAt: DateTime.SpecifyKind(
                    reader.GetDateTime(7), DateTimeKind.Utc)));
        }

        return messages;
    }
}