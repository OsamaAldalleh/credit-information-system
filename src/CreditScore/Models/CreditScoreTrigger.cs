namespace CreditScore.Models;

public enum CreditScoreTrigger
{
    LoanCreated,
    LoanClosed,
    PaymentsReceived,
    LitigationRecorded,
    VerdictRecorded,
    NightlyJob,
    OnDemand
}
