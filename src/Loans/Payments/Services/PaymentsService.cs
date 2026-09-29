using Common.Api.Responses;
using Common.Exceptions;
using EntityFramework.Exceptions.Common;
using Loans.Base.Models;
using Loans.Common.Data;
using Loans.Payments.Errors;
using Loans.Payments.Mappers;
using Loans.Payments.Models;
using Loans.Payments.Payloads;
using Microsoft.EntityFrameworkCore;
using Common.Contracts;
using Common.Security;
using MassTransit;

namespace Loans.Payments.Services;

public class PaymentsService(LoansDbContext loansDb, IPublishEndpoint publishEndpoint, CurrentUser currentUser)
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
        if (currentUser.IsScopedToInstitution && loan.InstitutionId != currentUser.InstitutionId)
        {
            throw ServiceException.Forbidden(PaymentErrors.LoanOfAnotherInstitution, loanId);
        }
        if(LoanStatus.Closed.Equals(loan.Status))
        {
            throw ServiceException.BadRequest(PaymentErrors.LoanClosed, loanId);
        }

        loansDb.LoanPayments.AddRange(payments.Select(p => p.ToEntity(loan)));
        await publishEndpoint.Publish(
            new CustomerCreditDataChanged(loan.CivilId, CreditDataChangeReason.PaymentsReceived, DateTimeOffset.UtcNow));
        try
        {
            await loansDb.SaveChangesAsync();
        } 
        catch(UniqueConstraintException)
        {
            throw ServiceException.BadRequest(PaymentErrors.DuplicatePaymentReference);
        }
    }

    public async Task<PageResult<LoanPaymentPayload>> GetLoanPaymentsAsync(Guid loanId, int page, int pageSize, string sort)
    {
        var query = loansDb.LoanPayments
            .AsNoTracking()
            .Where(p => p.LoanId == loanId);

        return await GetPaymentsPageAsync(query, page, pageSize, sort);
    }

    public async Task<LoanPaymentPayload> GetPaymentByReferenceAsync(string paymentReference)
    {
        var query = loansDb.LoanPayments
            .AsNoTracking()
            .Where(p => p.PaymentReference == paymentReference);

        if (currentUser.IsScopedToInstitution)
        {
            query = query.Where(p => p.InstitutionId == currentUser.InstitutionId);
        }

        // References are only unique per institution, so a bureau lookup can match several; the latest is returned.
        var payment = await query
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (payment is null)
        {
            throw ServiceException.NotFound(PaymentErrors.PaymentReferenceNotFound, paymentReference);
        }

        return payment.ToPayload();
    }

    public async Task<LoanPaymentPayload> GetPaymentAsync(Guid paymentId)
    {
        var payment = await loansDb.LoanPayments
            .FindAsync(paymentId);

        if (payment is null)
        {
            throw ServiceException.NotFound(PaymentErrors.PaymentNotFound, paymentId);
        }

        return payment.ToPayload();
    }

    public async Task<PageResult<LoanPaymentPayload>> GetCustomerPaymentsAsync(
        string civilId, LoanStatus? loanStatus, int page, int pageSize, string sort)
    {
        var query = loansDb.LoanPayments
            .AsNoTracking()
            .Where(p => p.CivilId == civilId);

        if (loanStatus is not null)
        {
            query = query.Where(p => loansDb.Loans.Any(l => l.Id == p.LoanId && l.Status == loanStatus.Value));
        }

        return await GetPaymentsPageAsync(query, page, pageSize, sort);
    }

    public async Task<IReadOnlyList<LoanPaymentPayload>> GetLatestCustomerPaymentsAsync(string civilId, int limit)
    {
        var payments = await loansDb.LoanPayments
            .AsNoTracking()
            .Where(p => p.CivilId == civilId)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.Id)
            .Take(limit)
            .ToListAsync();

        return payments.Select(p => p.ToPayload()).ToList();
    }

    private async Task<PageResult<LoanPaymentPayload>> GetPaymentsPageAsync(
        IQueryable<LoanPayment> query, int page, int pageSize, string sort)
    {
        var offset = (long)page * pageSize;

        if (offset > int.MaxValue)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("page", "Requested page is too large.")]);
        }

        var totalElements = await query.CountAsync();

        var orderedQuery = sort == "asc"
            ? query.OrderBy(p => p.PaymentDate).ThenBy(p => p.Id)
            : query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id);

        var payments = await orderedQuery
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LoanPaymentPayload>
        {
            Content = payments.Select(p => p.ToPayload()).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
    }
}
