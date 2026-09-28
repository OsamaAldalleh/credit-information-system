using Common.Api.Responses;
using Common.Exceptions;
using EntityFramework.Exceptions.Common;
using Loans.Base.Models;
using Loans.Common.Data;
using Loans.Payments.Errors;
using Loans.Payments.Mappers;
using Loans.Payments.Payloads;

namespace Loans.Payments.Services;

public class PaymentsService(LoansDbContext loansDb)
{
    public async Task UploadPaymentsAsync(Guid loanId, IReadOnlyList<UploadLoanPaymentPayload> payments)
    {
        if (payments.Any(p => p is null))
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("payments", "Payments must not contain null entries.")]);
        }
        
        var loan = await loansDb.Loans.FindAsync(loanId);
        if(loan is null)
        {
            throw ServiceException.BadRequest(PaymentErrors.LoanNotFound, loanId);
        }
        if(LoanStatus.Closed.Equals(loan.Status))
        {
            throw ServiceException.BadRequest(PaymentErrors.LoanClosed, loanId);
        }

        loansDb.LoanPayments.AddRange(payments.Select(p => p.ToEntity(loan)));
        try
        {
            await loansDb.SaveChangesAsync();   
        } 
        catch(UniqueConstraintException)
        {
            throw ServiceException.BadRequest(PaymentErrors.DuplicatePaymentReference);
        }
    }
}