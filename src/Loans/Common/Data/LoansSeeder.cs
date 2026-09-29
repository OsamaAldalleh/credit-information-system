using Loans.Base.Models;
using Loans.Base.Services;
using Loans.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Common.Data;

public static class LoansSeeder
{
    private static readonly Guid InstitutionId = Guid.Parse("01a0ea76-0c00-7000-8000-000000000001");
    private static readonly Guid PreviousSeedInstitutionId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public static void Seed(LoansDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var loans = new List<Loan>();
        var payments = new List<LoanPayment>();

        AddLoan(loans, payments, 1, 1, "DEMO-CURRENT", today.AddDays(-40), 12, PaymentFrequency.Monthly, 100m, 100m);
        AddLoan(loans, payments, 2, 2, "DEMO-DELINQUENT", today.AddDays(-40), 12, PaymentFrequency.Monthly, 100m);
        AddLoan(loans, payments, 3, 1, "DEMO-PAID-AHEAD", today.AddDays(-40), 12, PaymentFrequency.Monthly, 400m);
        AddLoan(loans, payments, 4, 1, "DEMO-PARTIALLY-AHEAD", today.AddDays(-40), 12, PaymentFrequency.Monthly, 250m);
        AddLoan(loans, payments, 5, 2, "DEMO-MATURED-OVERDUE", today.AddMonths(-6), 3, PaymentFrequency.Monthly, 150m);
        AddLoan(loans, payments, 6, 1, "DEMO-DUE-TODAY", today, 12, PaymentFrequency.Monthly);
        AddLoan(loans, payments, 7, 1, "DEMO-DUE-TODAY-PARTIAL", today, 12, PaymentFrequency.Monthly, 40m);
        AddLoan(loans, payments, 8, 1, "DEMO-NOT-STARTED", today.AddDays(14), 12, PaymentFrequency.Monthly);
        AddLoan(loans, payments, 9, 2, "DEMO-OVERDUE-PARTIAL", today.AddDays(-1), 12, PaymentFrequency.Monthly, 40m);
        AddLoan(loans, payments, 11, 1, "DEMO-SETTLED-OPEN", today.AddDays(-40), 3, PaymentFrequency.Monthly, 300m);
        AddLoan(loans, payments, 12, 1, "DEMO-OVERPAID-OPEN", today.AddDays(-40), 3, PaymentFrequency.Monthly, 350m);
        var closed = AddLoan(loans, payments, 13, 1, "DEMO-SETTLED-CLOSED", today.AddMonths(-6), 3, PaymentFrequency.Monthly, 300m);
        closed.Status = LoanStatus.Closed;
        closed.ClosedAt = AtUtcMidnight(today);
        closed.ClosureReason = "Fully repaid demo loan";
        AddLoan(loans, payments, 14, 3, "DEMO-WEEKLY", today.AddDays(-14), 8, PaymentFrequency.Weekly, 200m);
        AddLoan(loans, payments, 15, 3, "DEMO-QUARTERLY", today.AddMonths(-3), 4, PaymentFrequency.Quarterly, 100m);
        AddLoan(loans, payments, 16, 3, "DEMO-YEARLY", today.AddYears(-1), 3, PaymentFrequency.Yearly, 150m);
        AddLoan(loans, payments, 17, 2, "DEMO-MATURED-UNPAID", today.AddMonths(-6), 3, PaymentFrequency.Monthly);
        AddLoan(loans, payments, 18, 1, "DEMO-MATURED-SETTLED", today.AddMonths(-6), 3, PaymentFrequency.Monthly, 300m);
        AddLoan(loans, payments, 19, 3, "DEMO-SINGLE-INSTALLMENT", today.AddDays(7), 1, PaymentFrequency.Monthly, 25m);
        AddLoan(loans, payments, 21, 1, "DEMO-FRACTIONAL-AHEAD", today.AddDays(-40), 12, PaymentFrequency.Monthly, 299.999m);

        
        AddLoan(loans, payments, 22, 3, "DEMO-MONTH-END", new DateOnly(today.Year, 1, 31), 36, PaymentFrequency.Monthly);
        var leapYear = today.Year;
        while (!DateTime.IsLeapYear(leapYear))
        {
            leapYear--;
        }
        AddLoan(loans, payments, 23, 3, "DEMO-LEAP-DAY", new DateOnly(leapYear, 2, 29), 8, PaymentFrequency.Yearly);
        AddLoan(loans, payments, 24, 3, "DEMO-QUARTER-END", new DateOnly(today.Year, 1, 31), 8, PaymentFrequency.Quarterly);

        int[] bucketDays = [1, 30, 31, 60, 61, 90, 91, 120, 121, 150, 151, 180, 181];
        for (var i = 0; i < bucketDays.Length; i++)
        {
            var days = bucketDays[i];
            AddLoan(loans, payments, 25 + i, 2, $"DEMO-DPD-{days}", today.AddDays(-days), 12, PaymentFrequency.Monthly);
        }

        // Recognize the previous fixtures too, without rewriting IDs or duplicating existing demo loans.
        var references = loans.Select(l => l.ExternalReference).ToArray();
        var existingReferences = db.Loans.AsNoTracking()
            .Where(l => l.InstitutionId == InstitutionId || l.InstitutionId == PreviousSeedInstitutionId)
            .Where(l => references.Contains(l.ExternalReference))
            .Select(l => l.ExternalReference)
            .ToHashSet();

        var missingLoans = loans.Where(l => !existingReferences.Contains(l.ExternalReference)).ToList();
        var missingIds = missingLoans.Select(l => l.Id).ToHashSet();

        // A loan and its initial ledger are one fixture: never restore payments the user changed or deleted.
        db.Loans.AddRange(missingLoans);
        db.LoanPayments.AddRange(payments.Where(p => missingIds.Contains(p.LoanId)));
        db.SaveChanges();
    }

    private static Loan AddLoan(List<Loan> loans, List<LoanPayment> payments, int number, int customerNumber, string reference, DateOnly firstDueDate,
            int tenor, PaymentFrequency frequency, params decimal[] amounts)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = firstDueDate < today ? firstDueDate.AddDays(-7) : today.AddDays(-7);
            var loan = new Loan
            {
                Id = Guid.Parse($"01a0ea76-0c00-7001-8000-{number:000000000000}"),
                CivilId = $"90000000000{customerNumber}",
                InstitutionId = InstitutionId,
                ExternalReference = reference,
                StartDate = startDate,
                FirstDueDate = firstDueDate,
                Tenor = tenor,
                Rate = 0m,
                Amount = tenor * 100m,
                InstallmentAmount = 100m,
                PaymentFrequency = frequency,
                Status = LoanStatus.Open,
                CreatedAt = AtUtcMidnight(startDate)
            };
            loans.Add(loan);

            for (var i = 0; i < amounts.Length; i++)
            {
                payments.Add(new LoanPayment
                {
                    Id = Guid.Parse($"01a0ea76-0c00-7002-8{number:000}-{i + 1:000000000000}"),
                    LoanId = loan.Id,
                    CivilId = loan.CivilId,
                    InstitutionId = loan.InstitutionId,
                    PaymentReference = $"{reference}-PAY-{i + 1}",
                    PaymentDate = today.AddDays(-1),
                    Amount = amounts[i],
                    CreatedAt = AtUtcMidnight(today.AddDays(-1)).AddSeconds(i)
                });
            }

            return loan;
        }
    private static DateTimeOffset AtUtcMidnight(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
}
