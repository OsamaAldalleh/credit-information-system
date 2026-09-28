using Litigations.Models;

namespace Litigations.Payloads;

public class LitigationPayload
{
    public Guid? Id { get; set; }
    public string? CourtCaseNumber { get; set; }
    public Guid? LoanId { get; set; }
    public string? CivilId { get; set; }
    public Guid? InstitutionId { get; set; }
    public LitigationStatus? Status { get; set; }
    public DateOnly? FiledDate { get; set; }
    public DateOnly? VerdictDate { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
