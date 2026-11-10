using System.ComponentModel.DataAnnotations;
using Sms.Api.Models;
using Xunit;

namespace Sms.Api.Tests;

public class SendMessageRequestTests
{
    [Theory]
    [InlineData("0501234567", "שלום", true)]
    [InlineData("+972501234567", "שלום", true)]
    [InlineData("050123", "שלום", false)]
    [InlineData("05012345678", "שלום", false)]
    [InlineData("", "שלום", false)]
    [InlineData("0501234567", "", false)]
    [InlineData("0501234567", "   ", false)]
    public void ValidatesPhoneAndBody(
        string to,
        string body,
        bool expected)
    {
        var request = new SendMessageRequest
        {
            To = to,
            Body = body
        };

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            new List<ValidationResult>(),
            validateAllProperties: true);

        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData(160, true)]
    [InlineData(161, false)]
    public void ValidatesBodyLength(int length, bool expected)
    {
        var request = new SendMessageRequest
        {
            To = "0501234567",
            Body = new string('א', length)
        };

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            new List<ValidationResult>(),
            validateAllProperties: true);

        Assert.Equal(expected, isValid);
    }
}