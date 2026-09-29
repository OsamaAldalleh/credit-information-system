namespace CreditScore.Clients.Litigations;

public sealed record GuiltyVerdict(Guid LoanId, DateOnly? VerdictDate);
