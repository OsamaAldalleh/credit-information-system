using Common.Api.Responses;
using Common.Exceptions;
using Common.Exceptions.Errors;
using EntityFramework.Exceptions.Common;
using Loans.Base.Clients;
using Loans.Base.Errors;
using Loans.Base.Mappers;
using Loans.Base.Models;
using Loans.Base.Payloads;
using Loans.Common.Clients.Customers;
using Loans.Common.Data;
using Microsoft.EntityFrameworkCore;
using Common.Contracts;
using Common.Security;
using MassTransit;

namespace Loans.Base.Services;

public class LoansService(
    LoansDbContext loansDb,
    CustomersClient customersClient,
    LitigationsClient litigationsClient,
    IPublishEndpoint publishEndpoint,
    CurrentUser currentUser)
{

    public async Task<LoanPayload> CreateLoanAsync(CreateLoanPayload payload)
    {
        if(payload.FirstDueDate < payload.StartDate)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("first_due_date", "first_due_date should be at or after start_date")]);
        }
        var institutionId = currentUser.InstitutionId ?? throw ServiceException.Unauthorized(GenericErrors.Unauthorized);

        var customer = await customersClient.GetCustomerAsync(payload.CivilId!)
            ?? throw ServiceException.BadRequest(LoanErrors.CustomerNotFound, payload.CivilId);

        if (customer.LoanEligibility != CustomerLoanEligibility.Eligible)
        {
            throw ServiceException.BadRequest(LoanErrors.CustomerNotEligible, payload.CivilId);
        }

        var loan = payload.ToEntity(institutionId);
        loansDb.Loans.Add(loan);
        await publishEndpoint.Publish(
            new CustomerCreditDataChanged(loan.CivilId, CreditDataChangeReason.LoanCreated, DateTimeOffset.UtcNow));

        try
        {
            await loansDb.SaveChangesAsync();   
        }
        catch (UniqueConstraintException)
        {
            var existingId = await loansDb.Loans
                .Where(l => l.InstitutionId == loan.InstitutionId && l.ExternalReference == loan.ExternalReference)
                .Select(l => l.Id)
                .FirstAsync();

            throw ServiceException.BadRequest(LoanErrors.LoanAlreadyExists, loan.ExternalReference)
                .WithDetails(new { ExistingLoanId = existingId });
        }

        return loan.ToPayload(currentUser);
    }

    public async Task<LoanPayload> GetLoanAsync(Guid loanId)
    {
        var loan = await loansDb.Loans.FindAsync(loanId)
            ?? throw ServiceException.NotFound(LoanErrors.LoanNotFound, loanId);

        return loan.ToPayload(currentUser);
    }

    public async Task<LoanPayload> CloseLoanAsync(Guid loanId, CloseLoanPayload payload)
    {
        var loan = await loansDb.Loans.FindAsync(loanId)
            ?? throw ServiceException.NotFound(LoanErrors.LoanNotFound, loanId);

        if (currentUser.IsScopedToInstitution && loan.InstitutionId != currentUser.InstitutionId)
        {
            throw ServiceException.Forbidden(LoanErrors.LoanOfAnotherInstitution, loanId);
        }
        if (loan.Status == LoanStatus.Closed)
        {
            throw ServiceException.BadRequest(LoanErrors.LoanAlreadyClosed, loanId);
        }

        loan.Status = LoanStatus.Closed;
        loan.ClosureReason = payload.ClosureReason;
        loan.ClosedAt = DateTimeOffset.UtcNow;
        await publishEndpoint.Publish(
            new CustomerCreditDataChanged(loan.CivilId, CreditDataChangeReason.LoanClosed, DateTimeOffset.UtcNow));
        await loansDb.SaveChangesAsync();

        return loan.ToPayload(currentUser);
    }

    public async Task<PageResult<LoanPayload>> GetCustomerLoansAsync(string civilId, LoanStatus? status, int page, int pageSize)
    {
        var offset = (long)page * pageSize;

        if (offset > int.MaxValue)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("page", "Requested page is too large.")]);
        }

        var query = loansDb.Loans
            .AsNoTracking()
            .Where(l => l.CivilId == civilId);

        if (status is not null)
        {
            query = query.Where(l => l.Status == status);
        }

        var totalElements = await query.CountAsync();

        var loans = await query
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LoanPayload>
        {
            Content = loans.Select(l => l.ToPayload(currentUser)).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
    }

    public async Task<TotalCustomerLoansAmountPayload> GetCustomerTotalLoansAmountAsync(string civilId, LoanStatus? status)
    {
        var legalLoanIds = await litigationsClient.GetLegalLoanIdsAsync(civilId) ?? [];
        
        var query = loansDb.Loans
            .AsNoTracking()
            .Where(l => l.CivilId == civilId)
            .Where(l => !legalLoanIds.Contains(l.Id));
        if(status is not null)
        {
            query = query.Where(l => l.Status == status);
        }

        var sum = await query
            .Select(l => l.Amount)
            .SumAsync();

        return new() 
        {
            TotalLoansAmount = sum
        };
    }

    public async Task<IReadOnlyList<DelinquentLoanPayload>> GetDelinquentLoansAsync(string civilId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var loans = await loansDb.Loans
            .AsNoTracking()
            .Where(l => l.CivilId == civilId && l.Status == LoanStatus.Open)
            .Select(l => new { Loan = l, Paid = loansDb.LoanPayments.Where(p => p.LoanId == l.Id).Sum(p => p.Amount) })
            .ToListAsync();

        return loans
            .Select(l => (l.Loan, Standing: LoanScheduleCalculator.Calculate(l.Loan, l.Paid, today)))
            .Where(l => l.Standing.IsDelinquent)
            .OrderByDescending(l => l.Standing.DaysPastDue)
            .Select(l => new DelinquentLoanPayload
            {
                LoanId = l.Loan.Id,
                ExternalReference = l.Loan.ExternalReference,
                InstallmentAmount = l.Loan.InstallmentAmount,
                OverdueAmount = l.Standing.OverdueAmount,
                OverdueInstallments = l.Standing.OverdueInstallments,
                OverdueSince = l.Standing.OverdueSince,
                DaysPastDue = l.Standing.DaysPastDue,
                DelinquencyBucket = LoanScheduleCalculator.DelinquencyBucket(l.Standing.DaysPastDue)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<NextPaymentPayload>> GetNextPaymentsAsync(string civilId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var loans = await loansDb.Loans
            .AsNoTracking()
            .Where(l => l.CivilId == civilId && l.Status == LoanStatus.Open)
            .Select(l => new { Loan = l, Paid = loansDb.LoanPayments.Where(p => p.LoanId == l.Id).Sum(p => p.Amount) })
            .ToListAsync();

        return loans
            .Select(l => (l.Loan, Standing: LoanScheduleCalculator.Calculate(l.Loan, l.Paid, today)))
            .Where(l => !l.Standing.IsSettled)
            // Loans with no installments left but money still owed have no next date; they are due now, so they come first.
            .OrderBy(l => l.Standing.NextDueDate ?? DateOnly.MinValue)
            .Select(l => new NextPaymentPayload
            {
                LoanId = l.Loan.Id,
                ExternalReference = l.Loan.ExternalReference,
                OverdueAmount = l.Standing.OverdueAmount,
                OverdueSince = l.Standing.OverdueSince,
                NextDueDate = l.Standing.NextDueDate,
                NextInstallmentAmount = l.Standing.NextInstallmentAmount,
                TotalDueByNextDate = l.Standing.TotalDueByNextDate
            })
            .ToList();
    }

    // Closed loans are included: a loan closed with money still owed (e.g. written off) still counts as delinquent.
    public async Task<IReadOnlyList<LoanCreditSummaryPayload>> GetCreditSummaryAsync(string civilId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var loans = await loansDb.Loans
            .AsNoTracking()
            .Where(l => l.CivilId == civilId)
            .Select(l => new { Loan = l, Paid = loansDb.LoanPayments.Where(p => p.LoanId == l.Id).Sum(p => p.Amount) })
            .ToListAsync();

        return loans
            .Select(l => new LoanCreditSummaryPayload
            {
                LoanId = l.Loan.Id,
                Amount = l.Loan.Amount,
                Status = l.Loan.Status,
                OverdueInstallments = LoanScheduleCalculator.Calculate(l.Loan, l.Paid, today).OverdueInstallments
            })
            .ToList();
    }
}