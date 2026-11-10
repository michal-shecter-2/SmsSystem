using System.ComponentModel.DataAnnotations;

namespace Sms.MockProvider.Models;

public class SendSmsRequest
{
    [Required]
    public string To { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}