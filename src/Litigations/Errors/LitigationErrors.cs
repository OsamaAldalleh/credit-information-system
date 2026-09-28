using Common.Exceptions.Errors;

namespace Litigations.Errors;

public static class LitigationErrors
{
    public static readonly ServiceError LitigationNotFound = new("LIT-1000", "Litigation case {0} is not found");
    public static readonly ServiceError LoanNotFound = new("LIT-1001", "Loan {0} is not found");
    public static readonly ServiceError CaseAlreadyExists = new("LIT-1002", "Court case {0} is already registered for loan {1}");
    public static readonly ServiceError LoanAlreadyHasGuiltyVerdict = new("LIT-1003", "Loan {0} already has a GUILTY verdict");
    public static readonly ServiceError VerdictAlreadyRecorded = new("LIT-1004", "Litigation case {0} already has a verdict");
    public static readonly ServiceError LoansServiceUnavailable = new("LIT-1005", "Loan information is currently unavailable, try again later");
}
