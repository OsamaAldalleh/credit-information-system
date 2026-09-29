using Common.Api.Responses;
using Common.Exceptions;
using EntityFramework.Exceptions.Common;
using Litigations.Clients.Loans;
using Litigations.Data;
using Litigations.Data.Configurations;
using Litigations.Errors;
using Litigations.Mappers;
using Litigations.Models;
using Litigations.Payloads;
using Microsoft.EntityFrameworkCore;
using Common.Contracts;
using MassTransit;

namespace Litigations.Services;

public class LitigationsService(LitigationsDbContext litigationsDb, LoansClient loansClient, IPublishEndpoint publishEndpoint)
{
    public async Task<LitigationPayload> CreateLitigationAsync(CreateLitigationPayload payload)
    {
        ValidateLitigation(payload.Status, payload.FiledDate, payload.VerdictDate);
        var loan = await loansClient.GetLoanAsync(payload.LoanId!.Value)
            ?? throw ServiceException.BadRequest(LitigationErrors.LoanNotFound, payload.LoanId);

        var hasGuiltyVerdict = await litigationsDb.Litigations
            .AnyAsync(l => l.LoanId == loan.Id && l.Status == LitigationStatus.Guilty);
        if (hasGuiltyVerdict)
        {
            throw ServiceException.BadRequest(LitigationErrors.LoanAlreadyHasGuiltyVerdict, loan.Id);
        }

        var litigation = payload.ToEntity(loan);
        litigationsDb.Litigations.Add(litigation);
        await publishEndpoint.Publish(
            new CustomerCreditDataChanged(litigation.CivilId, CreditDataChangeReason.LitigationRecorded, DateTimeOffset.UtcNow));

        try
        {
            await litigationsDb.SaveChangesAsync();
        }
        catch (UniqueConstraintException ex) when (ex.ConstraintName == LitigationConfiguration.GuiltyVerdictPerLoanIndex)
        {
            throw ServiceException.BadRequest(LitigationErrors.LoanAlreadyHasGuiltyVerdict, loan.Id);
        }
        catch (UniqueConstraintException)
        {
            throw ServiceException.BadRequest(LitigationErrors.CaseAlreadyExists, litigation.CourtCaseNumber, loan.Id);
        }

        return litigation.ToPayload();
    }

    public async Task<LitigationPayload> UpdateLitigationStatusAsync(Guid litigationId, UpdateLitigationStatusPayload payload)
    {
        var litigation = await litigationsDb.Litigations.FindAsync(litigationId);
        if (litigation is null)
        {
            throw ServiceException.NotFound(LitigationErrors.LitigationNotFound, litigationId);
        }
        if (litigation.Status != LitigationStatus.Pending)
        {
            throw ServiceException.BadRequest(LitigationErrors.VerdictAlreadyRecorded, litigationId);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (payload.Status == LitigationStatus.Pending)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("status", "status must be GUILTY or INNOCENT")]);
        }
        if (payload.VerdictDate < litigation.FiledDate)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date should be at or after filed_date")]);
        }
        if (payload.VerdictDate > today)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date cannot be in the future")]);
        }

        litigation.Status = payload.Status!.Value;
        litigation.VerdictDate = payload.VerdictDate;
        litigation.UpdatedAt = DateTimeOffset.UtcNow;
        await publishEndpoint.Publish(
            new CustomerCreditDataChanged(litigation.CivilId, CreditDataChangeReason.VerdictRecorded, DateTimeOffset.UtcNow));

        try
        {
            await litigationsDb.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            throw ServiceException.BadRequest(LitigationErrors.LoanAlreadyHasGuiltyVerdict, litigation.LoanId);
        }

        return litigation.ToPayload();
    }

    public async Task<LitigationPayload> GetLitigationAsync(Guid litigationId)
    {
        var litigation = await litigationsDb.Litigations.FindAsync(litigationId);
        if (litigation is null)
        {
            throw ServiceException.NotFound(LitigationErrors.LitigationNotFound, litigationId);
        }

        return litigation.ToPayload();
    }

    public async Task<IReadOnlyList<LitigationPayload>> GetLoanLitigationsAsync(Guid loanId)
    {
        var litigations = await litigationsDb.Litigations
            .AsNoTracking()
            .Where(l => l.LoanId == loanId)
            .OrderByDescending(l => l.FiledDate)
            .ThenByDescending(l => l.Id)
            .ToListAsync();

        return litigations.Select(l => l.ToPayload()).ToList();
    }

    public async Task<PageResult<LitigationPayload>> GetCustomerLitigationsAsync(
        string civilId, LitigationStatus? status, int page, int pageSize)
    {
        var offset = (long)page * pageSize;

        if (offset > int.MaxValue)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("page", "Requested page is too large.")]);
        }

        var query = litigationsDb.Litigations
            .AsNoTracking()
            .Where(l => l.CivilId == civilId);

        if (status is not null)
        {
            query = query.Where(l => l.Status == status);
        }

        var totalElements = await query.CountAsync();

        var litigations = await query
            .OrderByDescending(l => l.FiledDate)
            .ThenByDescending(l => l.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LitigationPayload>
        {
            Content = litigations.Select(l => l.ToPayload()).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
    }

    // A loan is in a legal state while any of its cases is PENDING or ended GUILTY.
    public async Task<IReadOnlyList<Guid>> GetLegalLoanIdsAsync(string civilId)
    {
        return await litigationsDb.Litigations
            .AsNoTracking()
            .Where(l => l.CivilId == civilId && (l.Status == LitigationStatus.Pending || l.Status == LitigationStatus.Guilty))
            .Select(l => l.LoanId)
            .Distinct()
            .ToListAsync();
    }

    private static void ValidateLitigation(LitigationStatus? litigationStatus, DateOnly? filedDate, DateOnly? verdictDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var status = litigationStatus ?? LitigationStatus.Pending;

        if (filedDate > today)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("filed_date", "filed_date cannot be in the future")]);
        }
        if (status == LitigationStatus.Pending && verdictDate is not null)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date must be empty while the case is PENDING")]);
        }
        if (status != LitigationStatus.Pending && verdictDate is null)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date is required for a GUILTY or INNOCENT verdict")]);
        }
        if (verdictDate < filedDate)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date should be at or after filed_date")]);
        }
        if (verdictDate > today)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("verdict_date", "verdict_date cannot be in the future")]);
        }
    }
}
