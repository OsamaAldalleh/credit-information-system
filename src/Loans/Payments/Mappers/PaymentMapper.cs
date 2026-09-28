using Loans.Base.Models;
using Loans.Payments.Models;
using Loans.Payments.Payloads;

namespace Loans.Payments.Mappers;

public static class PaymentMapper
{
    // todo: map institution id to "Other Banks" when a bank is querying loans of not its own
    public static LoanPayment ToEntity(this UploadLoanPaymentPayload payment, Loan loan) => new()
    {
        Id = Guid.CreateVersion7(),
        CivilId = loan.CivilId,
        PaymentReference = payment.PaymentReference!,
        LoanId = loan.Id,
        InstitutionId = loan.InstitutionId,
        Amount = payment.Amount!.Value,
        PaymentDate = payment.PaymentDate!.Value,
        CreatedAt = DateTimeOffset.UtcNow
    };
}