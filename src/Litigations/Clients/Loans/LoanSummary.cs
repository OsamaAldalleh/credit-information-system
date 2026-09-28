namespace Litigations.Clients.Loans;

// Only the fields Litigations needs from the Loans service; services share an API contract, not code.
public sealed record LoanSummary(Guid Id, string CivilId, Guid InstitutionId);
