using Common.Exceptions.Errors;

namespace Loans.Payments.Errors;

public static class PaymentErrors
{
    public static readonly ServiceError LoanNotFound = new("PMNT-1000", "Could not process payments as Loan {0} could not be found");
    public static readonly ServiceError LoanClosed = new("PMNT-1001", "Could not process payments as Loan {0} is already closed");
    public static readonly ServiceError DuplicatePaymentReference = new("PMNT-1002", "Processing Failed, one or more payment references are duplicate");
    public static readonly ServiceError PaymentNotFound = new("PMNT-1003", "Payment {0} is not found");
    public static readonly ServiceError PaymentReferenceNotFound = new("PMNT-1004", "Payment reference {0} is not found");
    public static readonly ServiceError LoanOfAnotherInstitution = new("PMNT-1005", "Could not process payments as Loan {0} belongs to another institution");
}
