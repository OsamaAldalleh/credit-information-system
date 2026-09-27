
using Common.Exceptions.Errors;

namespace Customers.Errors;

public static class CustomerErrors
{
    public static readonly ServiceError CustomerNotFound = new("CUS-1000", "Customer with Civil ID {0} is not found");
    public static readonly ServiceError CustomerAlreadyExists = new("CUS-1001", "Civil ID {0} is already registered");
}