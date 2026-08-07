using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManagementTool.Application.DTOs.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        if (!string.IsNullOrEmpty(Password) && Encoding.UTF8.GetByteCount(Password) > 72) { 
            yield return new ValidationResult
                ("Password must not exceed 72 bytes in UTF-8 encoding.", 
                new[] { nameof(Password) });
        }
    }
}