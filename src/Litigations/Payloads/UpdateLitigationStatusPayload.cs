using System.ComponentModel.DataAnnotations;
using Litigations.Models;

namespace Litigations.Payloads;

public class UpdateLitigationStatusPayload
{
    [Required]
    [EnumDataType(typeof(LitigationStatus))]
    public LitigationStatus? Status { get; set; }

    [Required]
    public DateOnly? VerdictDate { get; set; }
}
