using Common.Api.Responses;
using Common.Exceptions;
using Common.Exceptions.Errors;
using Common.Security;
using Customers.Data;
using Customers.Errors;
using Customers.Mappers;
using Customers.Models;
using Customers.Payloads;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace Customers.Services;

public class CustomersService(CustomersDbContext customersDb, CurrentUser currentUser)
{

    public async Task<CustomerPayload> CreateCustomerAsync(CustomerPayload customerPayload)
    {
        var customer = customerPayload.ToEntity();
        customersDb.Customers.Add(customer);

        try
        {
            await customersDb.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            throw ServiceException.BadRequest(CustomerErrors.CustomerAlreadyExists, customer.CivilId);
        }
        return customer.ToPayload();
    }

    public async Task<CustomerPayload> GetCustomerAsync(string civilId)
    {
        var customer = await customersDb.Customers.FindAsync(civilId);
        if (customer is null)
        {
            throw ServiceException.NotFound(CustomerErrors.CustomerNotFound, civilId);
        }
        return customer.ToPayload();
    }

    public async Task<CustomerPayload> UpdateCustomerLoanEligibilityAsync(string civilId, UpdateEligibilityPayload payload)
    {
        var customer = await customersDb.Customers.FindAsync(civilId);
        if (customer is null)
        {
            throw ServiceException.NotFound(CustomerErrors.CustomerNotFound, civilId);
        }
        if (customer.LoanEligibility.Equals(payload.LoanEligibility))
        {
            return customer.ToPayload();
        }

        var changedBy = currentUser.UserId ?? throw ServiceException.Unauthorized(GenericErrors.Unauthorized);
        var now = DateTimeOffset.UtcNow;
        customer.LoanEligibility = payload.LoanEligibility!.Value;
        customer.UpdatedAt = now;
        CustomerLoanEligibilityHistory history = new()
        {
            Id = Guid.CreateVersion7(),
            CivilId = customer.CivilId,
            LoanEligibility = payload.LoanEligibility!.Value,
            Reason = payload.Reason,
            ChangedBy = changedBy,
            CreatedAt = now,
        };
        customersDb.CustomerLoanEligibilityHistory.Add(history);
        await customersDb.SaveChangesAsync();
        return customer.ToPayload();
    }

    public async Task<CustomerLoanEligibilityHistoryResponse> GetLoanEligibilityHistoryAsync(
        string civilId,
        int page,
        int pageSize)
    {
        var offset = (long)page * pageSize;

        if (offset > int.MaxValue)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("page", "Requested page is too large.")]);
        }

        var query = customersDb.CustomerLoanEligibilityHistory
            .AsNoTracking()
            .Where(h => h.CivilId == civilId);

        var totalElements = await query.CountAsync();

        var history = await query
            .OrderByDescending(h => h.CreatedAt)
            .ThenByDescending(h => h.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync();

        PageResult<CustomerLoanEligibilityHistoryPayload> result = new()
        {
            Content = history.Select(h => h.ToPayload()).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
        return new()
        {
            CivilId = civilId,
            Result = result
        };
    }

}