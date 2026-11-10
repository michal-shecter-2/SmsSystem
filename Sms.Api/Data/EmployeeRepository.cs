using System.Data;
using Microsoft.Data.SqlClient;
using Sms.Api.Models;

namespace Sms.Api.Data;

public class EmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SmsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'SmsDb' is missing.");
    }

    public async Task<List<EmployeeResponse>> ListAsync()
    {
        const string sql = """
            SELECT Id, FirstName, LastName, Address
            FROM dbo.Employees
            ORDER BY FirstName, LastName, Address;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var employees = new List<EmployeeResponse>();

        while (await reader.ReadAsync())
        {
            employees.Add(new EmployeeResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)));
        }

        return employees;
    }

    public async Task<List<string>> GetPhonesAsync(int employeeId)
    {
        const string sql = """
            SELECT DISTINCT PhoneNumber
            FROM dbo.EmployeePhones
            WHERE EmployeeId = @EmployeeId
            ORDER BY PhoneNumber;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;

        await using var reader = await command.ExecuteReaderAsync();
        var phones = new List<string>();

        while (await reader.ReadAsync())
        {
            phones.Add(reader.GetString(0));
        }

        return phones;
    }
}
