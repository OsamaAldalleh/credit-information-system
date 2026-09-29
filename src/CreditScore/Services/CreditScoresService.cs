using Common.Api.Responses;
using Common.Exceptions;
using CreditScore.Clients.Customers;
using CreditScore.Clients.Litigations;
using CreditScore.Clients.Loans;
using CreditScore.Data;
using CreditScore.Errors;
using CreditScore.Mappers;
using CreditScore.Models;
using CreditScore.Payloads;
using Microsoft.EntityFrameworkCore;

namespace CreditScore.Services;

public class CreditScoresService(
    CreditScoreDbContext creditScoreDb,
    CustomersClient customersClient,
    LoansClient loansClient,
    LitigationsClient litigationsClient)
{
    private const decimal LargeLoanAmount = 10000;

    public async Task<CreditScorePayload> GetCreditScoreAsync(string civilId)
    {
        var creditScore = await creditScoreDb.CreditScores.FindAsync(civilId);
        if (creditScore is not null)
        {
            return creditScore.ToPayload();
        }

        if (!await customersClient.CustomerExistsAsync(civilId))
        {
            throw ServiceException.NotFound(CreditScoreErrors.CustomerNotFound, civilId);
        }

        return await RecalculateCreditScoreAsync(civilId, CreditScoreTrigger.OnDemand);
    }

    public async Task<PageResult<CreditScoreHistoryPayload>> GetCreditScoreHistoryAsync(string civilId, int page, int pageSize)
    {
        var offset = (long)page * pageSize;

        if (offset > int.MaxValue)
        {
            throw ServiceException.ValidationFailed(
                [new ValidationError("page", "Requested page is too large.")]);
        }

        var query = creditScoreDb.CreditScoreHistory
            .AsNoTracking()
            .Where(h => h.CivilId == civilId);

        var totalElements = await query.CountAsync();

        var history = await query
            .OrderByDescending(h => h.CalculatedAt)
            .ThenByDescending(h => h.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<CreditScoreHistoryPayload>
        {
            Content = history.Select(h => h.ToPayload()).ToList(),
            Page = page,
            Size = pageSize,
            TotalElements = totalElements
        };
    }

    public async Task<CreditScorePayload> RecalculateCreditScoreAsync(string civilId, CreditScoreTrigger trigger)
    {
        await using var transaction = await creditScoreDb.Database.BeginTransactionAsync();

        // One recalculation per customer at a time,
        // so a slower recalculation can never overwrite a newer result.
        await creditScoreDb.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({civilId}, 0))");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var loans = await loansClient.GetCreditSummaryAsync(civilId);
        var legalLoanIds = await litigationsClient.GetLegalLoanIdsAsync(civilId);
        var guiltyVerdicts = await litigationsClient.GetGuiltyVerdictsAsync(civilId);

        var activeLoans = loans.Count(l => l.Status == LoanStatus.Open);
        var delinquencies = loans.Sum(l => l.OverdueInstallments);
        var loansInLitigation = legalLoanIds.Count;
        var recentGuiltyVerdicts = guiltyVerdicts.Count(v =>
        {
            var loan = loans.FirstOrDefault(l => l.LoanId == v.LoanId);
            if (loan is null || v.VerdictDate is null)
            {
                return false;
            }

            var countedSince = loan.Amount >= LargeLoanAmount ? today.AddYears(-3) : today.AddYears(-1);
            return v.VerdictDate > countedSince;
        });

        var grade = (delinquencies, recentGuiltyVerdicts, loansInLitigation, activeLoans) switch
        {
            ( > 3, _, _, _) => CreditGrade.F,
            (_, > 0, _, _) => CreditGrade.F,
            // Not covered by the doc: 1-3 delinquencies with a loan in litigation is graded F.
            ( > 0, _, > 0, _) => CreditGrade.F,
            ( > 0, _, _, _) => CreditGrade.C,
            (_, _, _, > 0) => CreditGrade.A,
            _ => CreditGrade.B
        };

        var now = DateTimeOffset.UtcNow;
        var creditScore = await creditScoreDb.CreditScores.FindAsync(civilId);
        var previousGrade = creditScore?.Grade;

        if (creditScore is null)
        {
            creditScore = new CustomerCreditScore { CivilId = civilId };
            creditScoreDb.CreditScores.Add(creditScore);
        }

        creditScore.Grade = grade;
        creditScore.ActiveLoans = activeLoans;
        creditScore.Delinquencies = delinquencies;
        creditScore.LoansInLitigation = loansInLitigation;
        creditScore.RecentGuiltyVerdicts = recentGuiltyVerdicts;
        creditScore.CalculatedAt = now;

        if (previousGrade != grade)
        {
            creditScoreDb.CreditScoreHistory.Add(new CreditScoreHistory
            {
                Id = Guid.CreateVersion7(),
                CivilId = civilId,
                Grade = grade,
                PreviousGrade = previousGrade,
                ActiveLoans = activeLoans,
                Delinquencies = delinquencies,
                LoansInLitigation = loansInLitigation,
                RecentGuiltyVerdicts = recentGuiltyVerdicts,
                Trigger = trigger,
                CalculatedAt = now
            });
        }

        await creditScoreDb.SaveChangesAsync();
        await transaction.CommitAsync();

        return creditScore.ToPayload();
    }
}
