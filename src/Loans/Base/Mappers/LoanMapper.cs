using Common.Security;
using Loans.Base.Models;
using Loans.Base.Payloads;

namespace Loans.Base.Mappers;

public static class LoanMapper
{
    private const string OtherBanks = "OTHER_BANKS";

    // Bank users see other banks' loans without learning which bank holds them.
    public static LoanPayload ToPayload(this Loan loan, CurrentUser currentUser)
    {
        var payload = loan.ToPayload();
        if (!currentUser.IsScopedToInstitution)
        {
            return payload;
        }

        if (loan.InstitutionId == currentUser.InstitutionId)
        {
            payload.InstitutionName = currentUser.InstitutionName;
        }
        else
        {
            payload.InstitutionId = null;
            payload.InstitutionName = OtherBanks;
        }

        return payload;
    }

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
