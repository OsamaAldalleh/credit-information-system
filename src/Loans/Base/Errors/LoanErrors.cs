using Common.Exceptions.Errors;

namespace Loans.Base.Errors;

public static class LoanErrors
{
    public static readonly ServiceError CustomerNotFound = new("LOAN-1000", "Customer with Civil ID {0} is not found");
    public static readonly ServiceError CustomerNotEligible = new("LOAN-1001", "Customer with Civil ID {0} is not eligible for a loan");
    public static readonly ServiceError CustomersServiceUnavailable = new("LOAN-1002", "Customer information is currently unavailable, try again later");
    public static readonly ServiceError LitigationsServiceUnavailable = new("LOAN-1003", "Litigations information is currently unavailable, try again later");
    public static readonly ServiceError LoanAlreadyExists = new("LOAN-1004", "Loan with external reference {0} already exists");
    public static readonly ServiceError LoanNotFound = new("LOAN-1005", "Loan {0} is not found");
    public static readonly ServiceError LoanAlreadyClosed = new("LOAN-1006", "Loan {0} is already closed");

}
