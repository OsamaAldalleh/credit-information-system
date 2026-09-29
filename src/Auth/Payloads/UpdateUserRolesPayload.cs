using System.ComponentModel.DataAnnotations;

namespace Auth.Payloads;

public class UpdateUserRolesPayload
{
    [Required]
    [MinLength(1)]
    public List<string>? Roles { get; set; }
}
