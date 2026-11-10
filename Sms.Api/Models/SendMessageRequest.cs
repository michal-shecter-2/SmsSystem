using System.ComponentModel.DataAnnotations;

namespace Sms.Api.Models;

public class SendMessageRequest
{
    [Required]
    [RegularExpression(
     @"\A(?:05[0-9]{8}|\+9725[0-9]{8})\z",
     ErrorMessage = "Enter an Israeli mobile number, such as 0501234567 or +972501234567.")]
    public string To { get; set; } = string.Empty;

    [Required]
    [StringLength(
        160,
        ErrorMessage = "Message must not exceed 160 characters.")]
    public string Body { get; set; } = string.Empty;
}