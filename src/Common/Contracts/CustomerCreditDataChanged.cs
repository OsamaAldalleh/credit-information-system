using MassTransit;

namespace Common.Contracts;

[EntityName("customer-credit-data-changed")]
public record CustomerCreditDataChanged(string CivilId, CreditDataChangeReason Reason, DateTimeOffset OccurredAt);

public enum CreditDataChangeReason
{
    LoanCreated,
    LoanClosed,
    PaymentsReceived,
    LitigationRecorded,
    VerdictRecorded
}
