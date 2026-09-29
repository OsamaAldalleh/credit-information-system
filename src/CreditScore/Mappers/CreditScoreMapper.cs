using CreditScore.Models;
using CreditScore.Payloads;

namespace CreditScore.Mappers;

public static class CreditScoreMapper
{
    public static CreditScorePayload ToPayload(this CustomerCreditScore creditScore) => new()
    {
        CivilId = creditScore.CivilId,
        Grade = creditScore.Grade,
        ActiveLoans = creditScore.ActiveLoans,
        Delinquencies = creditScore.Delinquencies,
        LoansInLitigation = creditScore.LoansInLitigation,
        RecentGuiltyVerdicts = creditScore.RecentGuiltyVerdicts,
        CalculatedAt = creditScore.CalculatedAt
    };

    public static CreditScoreHistoryPayload ToPayload(this CreditScoreHistory history) => new()
    {
        Id = history.Id,
        Grade = history.Grade,
        PreviousGrade = history.PreviousGrade,
        ActiveLoans = history.ActiveLoans,
        Delinquencies = history.Delinquencies,
        LoansInLitigation = history.LoansInLitigation,
        RecentGuiltyVerdicts = history.RecentGuiltyVerdicts,
        Trigger = history.Trigger,
        CalculatedAt = history.CalculatedAt
    };
}
