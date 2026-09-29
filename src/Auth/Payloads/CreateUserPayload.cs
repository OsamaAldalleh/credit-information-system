using System.ComponentModel.DataAnnotations;

namespace Auth.Payloads;

public class CreateUserPayload
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,64}$", ErrorMessage = "username must be 3-64 letters, digits, dots, dashes or underscores")]
    public string? Username { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string? Password { get; set; }

    [Required]
    public Guid? InstitutionId { get; set; }

    [Required]
    [MinLength(1)]
    public List<string>? Roles { get; set; }
}
