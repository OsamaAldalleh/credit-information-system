using Loans.Base.Models;
using Loans.Base.Payloads;

namespace Loans.Base.Mappers;

public static class LoanMapper
{
    // todo: map institution id to "Other Banks" when a bank is querying loans of not its own
    public static LoanPayload ToPayload(this Loan loan) => new()
    {
        Id = loan.Id,
        CivilId = loan.CivilId,
        InstitutionId = loan.InstitutionId,
        ExternalReference = loan.ExternalReference,
        StartDate = loan.StartDate,
        FirstDueDate = loan.FirstDueDate,
        Tenor = loan.Tenor,
        Rate = loan.Rate,
        Amount = loan.Amount,
        InstallmentAmount = loan.InstallmentAmount,
        PaymentFrequency = loan.PaymentFrequency,
        Status = loan.Status,
        ClosureReason = loan.ClosureReason,
        CreatedAt = loan.CreatedAt,
        ClosedAt = loan.ClosedAt
    };

    // Payload fields are non-null here: [Required] rejects the request before it reaches the service.
    // The institution comes from the caller's identity, never from the request body.
    public static Loan ToEntity(this CreateLoanPayload payload, Guid institutionId) => new()
    {
        Id = Guid.CreateVersion7(),
        CivilId = payload.CivilId!,
        InstitutionId = institutionId,
        ExternalReference = payload.ExternalReference!,
        StartDate = payload.StartDate!.Value,
        FirstDueDate = payload.FirstDueDate!.Value,
        Tenor = payload.Tenor!.Value,
        Rate = payload.Rate!.Value,
        Amount = payload.Amount!.Value,
        InstallmentAmount = payload.InstallmentAmount!.Value,
        PaymentFrequency = payload.PaymentFrequency!.Value,
        Status = LoanStatus.Open,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
