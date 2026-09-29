using Common.Exceptions.Errors;

namespace CreditScore.Errors;

public static class CreditScoreErrors
{
    public static readonly ServiceError LoansServiceUnavailable = new("SCORE-1000", "Loan information is currently unavailable, try again later");
    public static readonly ServiceError LitigationsServiceUnavailable = new("SCORE-1001", "Litigation information is currently unavailable, try again later");
    public static readonly ServiceError CustomersServiceUnavailable = new("SCORE-1002", "Customer information is currently unavailable, try again later");
    public static readonly ServiceError CustomerNotFound = new("SCORE-1003", "Customer with Civil ID {0} is not found");
}
