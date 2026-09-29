using System.ComponentModel.DataAnnotations;
using Auth.Models;

namespace Auth.Payloads;

public class CreateInstitutionPayload
{
    [Required]
    [StringLength(255)]
    public string? Name { get; set; }

    [Required]
    [EnumDataType(typeof(InstitutionType))]
    public InstitutionType? Type { get; set; }
}
