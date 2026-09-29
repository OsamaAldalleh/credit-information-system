using Loans.Base.Models;
using Loans.Payments.Models;
using Loans.Payments.Payloads;

namespace Loans.Payments.Mappers;

public static class PaymentMapper
{
    public static LoanPaymentPayload ToPayload(this LoanPayment payment) => new()
    {
        Id = payment.Id,
        LoanId = payment.LoanId,
        CivilId = payment.CivilId,
        PaymentReference = payment.PaymentReference,
        PaymentDate = payment.PaymentDate,
        Amount = payment.Amount,
        CreatedAt = payment.CreatedAt
    };

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
