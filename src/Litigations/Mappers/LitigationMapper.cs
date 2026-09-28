using Litigations.Clients.Loans;
using Litigations.Models;
using Litigations.Payloads;

namespace Litigations.Mappers;

public static class LitigationMapper
{
    public static LitigationPayload ToPayload(this Litigation litigation) => new()
    {
        Id = litigation.Id,
        CourtCaseNumber = litigation.CourtCaseNumber,
        LoanId = litigation.LoanId,
        CivilId = litigation.CivilId,
        InstitutionId = litigation.InstitutionId,
        Status = litigation.Status,
        FiledDate = litigation.FiledDate,
        VerdictDate = litigation.VerdictDate,
        CreatedAt = litigation.CreatedAt,
        UpdatedAt = litigation.UpdatedAt
    };

    // Customer and institution come from the loan itself, never from the request body.
    public static Litigation ToEntity(this CreateLitigationPayload payload, LoanSummary loan)
    {
        var now = DateTimeOffset.UtcNow;

        return new Litigation
        {
            Id = Guid.CreateVersion7(),
            CourtCaseNumber = payload.CourtCaseNumber!,
            LoanId = loan.Id,
            CivilId = loan.CivilId,
            InstitutionId = loan.InstitutionId,
            Status = payload.Status ?? LitigationStatus.Pending,
            FiledDate = payload.FiledDate!.Value,
            VerdictDate = payload.VerdictDate,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
