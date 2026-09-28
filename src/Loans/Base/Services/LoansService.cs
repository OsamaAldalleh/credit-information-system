using Common.Api.Responses;
using Common.Exceptions;
using EntityFramework.Exceptions.Common;
using Loans.Base.Errors;
using Loans.Base.Mappers;
using Loans.Base.Models;
using Loans.Base.Payloads;
using Loans.Common.Clients.Customers;
using Loans.Common.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Loans.Base.Services;

public class LoansService(LoansDbContext loansDb, CustomersClient customersClient)
{

    public async Task<LoanPayload> CreateLoanAsync(CreateLoanPayload payload)
    {
        if(payload.FirstDueDate < payload.StartDate)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("first_due_date", "first_due_date should be at or after start_date")]);
        }
        var customer = await customersClient.GetCustomerAsync(payload.CivilId!)
            ?? throw ServiceException.BadRequest(LoanErrors.CustomerNotFound, payload.CivilId);

        if (customer.LoanEligibility != CustomerLoanEligibility.Eligible)
        {
            throw ServiceException.BadRequest(LoanErrors.CustomerNotEligible, payload.CivilId);
        }

        // todo: get institution id from headers
        var loan = payload.ToEntity(Guid.CreateVersion7());
        loansDb.Loans.Add(loan);

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

        return loan.ToPayload();
    }

    // todo: once auth is done, non-bureau callers see InstitutionId only for their own institution's loans;
    //       every other loan's institution is reported as OTHER_BANKS.
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
            Content = loans.Select(l => l.ToPayload()).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
    }
}