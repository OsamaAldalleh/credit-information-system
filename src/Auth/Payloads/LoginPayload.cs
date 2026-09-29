using System.ComponentModel.DataAnnotations;

namespace Auth.Payloads;

public class LoginPayload
{
    [Required]
    [StringLength(64)]
    public string? Username { get; set; }

    [Required]
    [StringLength(128)]
    public string? Password { get; set; }
}
