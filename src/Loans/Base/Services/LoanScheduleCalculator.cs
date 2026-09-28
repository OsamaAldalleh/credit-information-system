using Loans.Base.Models;

namespace Loans.Base.Services;

// Delinquency and upcoming payments are derived from the loan terms and the append-only payment ledger, never stored.
// Payments settle the oldest unpaid installment first; an installment is late from the day after its due date.
public static class LoanScheduleCalculator
{
    public const int DelinquencyThresholdDays = 1;

    private static readonly TimeSpan KuwaitOffset = TimeSpan.FromHours(3);

    public static DateOnly KuwaitToday() =>
        DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(KuwaitOffset).DateTime);

    public static DateOnly DueDate(Loan loan, int installmentIndex) => loan.PaymentFrequency switch
    {
        PaymentFrequency.Weekly => loan.FirstDueDate.AddDays(7 * installmentIndex),
        PaymentFrequency.Monthly => loan.FirstDueDate.AddMonths(installmentIndex),
        PaymentFrequency.Quarterly => loan.FirstDueDate.AddMonths(3 * installmentIndex),
        PaymentFrequency.Yearly => loan.FirstDueDate.AddYears(installmentIndex),
        _ => throw new ArgumentOutOfRangeException(nameof(loan), loan.PaymentFrequency, "Unknown payment frequency")
    };

    public static LoanStanding Calculate(Loan loan, decimal paidSoFar, DateOnly today)
    {
        var installmentAmount = loan.InstallmentAmount;

        var pastDueInstallments = 0;
        while (pastDueInstallments < loan.Tenor && DueDate(loan, pastDueInstallments) < today)
        {
            pastDueInstallments++;
        }

        var coveredInstallments = Math.Floor(paidSoFar / installmentAmount);
        var fullyPaidInstallments = coveredInstallments >= loan.Tenor ? loan.Tenor : (int)coveredInstallments;

        var overdueAmount = Math.Max(0, pastDueInstallments * installmentAmount - paidSoFar);
        DateOnly? overdueSince = overdueAmount > 0 ? DueDate(loan, fullyPaidInstallments) : null;
        var daysPastDue = overdueSince is { } since ? today.DayNumber - since.DayNumber : 0;

        var nextInstallmentIndex = Math.Max(fullyPaidInstallments, pastDueInstallments);
        var hasNextInstallment = nextInstallmentIndex < loan.Tenor;
        var nextInstallmentAmount = hasNextInstallment
            ? Math.Min(installmentAmount, (nextInstallmentIndex + 1) * installmentAmount - paidSoFar)
            : 0;

        return new LoanStanding
        {
            OverdueAmount = overdueAmount,
            OverdueInstallments = Math.Max(0, pastDueInstallments - fullyPaidInstallments),
            OverdueSince = overdueSince,
            DaysPastDue = daysPastDue,
            IsDelinquent = daysPastDue >= DelinquencyThresholdDays,
            NextDueDate = hasNextInstallment ? DueDate(loan, nextInstallmentIndex) : null,
            NextInstallmentAmount = nextInstallmentAmount,
            IsSettled = paidSoFar >= loan.Tenor * installmentAmount
        };
    }

    public static string DelinquencyBucket(int daysPastDue) => daysPastDue switch
    {
        <= 0 => "CURRENT",
        <= 30 => "1-30",
        <= 60 => "31-60",
        <= 90 => "61-90",
        <= 120 => "91-120",
        <= 150 => "121-150",
        <= 180 => "151-180",
        _ => "180+"
    };
}

public sealed record LoanStanding
{
    public decimal OverdueAmount { get; init; }
    public int OverdueInstallments { get; init; }
    public DateOnly? OverdueSince { get; init; }
    public int DaysPastDue { get; init; }
    public bool IsDelinquent { get; init; }
    public DateOnly? NextDueDate { get; init; }
    public decimal NextInstallmentAmount { get; init; }
    public decimal TotalDueByNextDate => OverdueAmount + NextInstallmentAmount;
    public bool IsSettled { get; init; }
}
