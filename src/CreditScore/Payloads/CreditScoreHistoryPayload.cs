using CreditScore.Models;

namespace CreditScore.Payloads;

public class CreditScoreHistoryPayload
{
    public Guid? Id { get; set; }
    public CreditGrade? Grade { get; set; }
    public CreditGrade? PreviousGrade { get; set; }
    public int? ActiveLoans { get; set; }
    public int? Delinquencies { get; set; }
    public int? LoansInLitigation { get; set; }
    public int? RecentGuiltyVerdicts { get; set; }
    public CreditScoreTrigger? Trigger { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
}
