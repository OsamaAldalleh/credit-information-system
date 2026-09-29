using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using Loans.Base.Models;
using Loans.Base.Payloads;
using Loans.Base.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Base.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController(LoansService loansService) : ControllerBase
{
    
    [HttpPost]
    [EndpointDescription("The loan belongs to the caller's bank (from the token). The customer must exist and be eligible for loans.")]
    public async Task<ActionResult<ApiResponse<LoanPayload>>> CreateLoan(CreateLoanPayload payload)
    {
        var loan = await loansService.CreateLoanAsync(payload);
        return ApiResponseBuilder.Ok(loan);
    }

    // The guid constraint keeps this route apart from the civil ID routes below.
    [HttpGet("{loanId:guid}", Name = nameof(GetLoan))]
    public async Task<ActionResult<ApiResponse<LoanPayload>>> GetLoan(Guid loanId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetLoanAsync(loanId));
    }

    [HttpPatch("{loanId:guid}/close")]
    public async Task<ActionResult<ApiResponse<LoanPayload>>> CloseLoan(Guid loanId, CloseLoanPayload payload)
    {
        return ApiResponseBuilder.Ok(await loansService.CloseLoanAsync(loanId, payload));
    }

    [HttpGet("{civilId}")]
    [EndpointDescription("Bank users see other banks' loans with institution_name OTHER_BANKS and no institution_id.")]
    public async Task<ActionResult<ApiResponse<PageResult<LoanPayload>>>> GetCustomerLoans(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LoanStatus))] LoanStatus? loanStatus = null,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCustomerLoansAsync(civilId, loanStatus, page, pageSize));
    }

    [HttpGet("{civilId}/total")]
    [EndpointDescription("Sum of loan amounts, excluding loans in legal state (a PENDING or GUILTY litigation). Counts open loans unless loanStatus says otherwise.")]
    public async Task<ActionResult<ApiResponse<TotalCustomerLoansAmountPayload>>> GetCustomerTotalLoansAmount(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LoanStatus))] LoanStatus loanStatus = LoanStatus.Open)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCustomerTotalLoansAmountAsync(civilId, loanStatus));
    }

    [HttpGet("{civilId}/delinquent")]
    [EndpointDescription("Open loans with at least one installment past its due date and not fully paid, most days past due first.")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DelinquentLoanPayload>>>> GetDelinquentLoans(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetDelinquentLoansAsync(civilId));
    }

    [HttpGet("{civilId}/next-payment")]
    [EndpointDescription("One entry per open, unsettled loan: overdue amount, next due date and the total to pay by that date, earliest first.")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NextPaymentPayload>>>> GetNextPayments(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetNextPaymentsAsync(civilId));
    }

    // Internal: used by the CreditScore service, never mapped in the gateway.
    [HttpGet("{civilId}/credit-summary")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LoanCreditSummaryPayload>>>> GetCreditSummary(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCreditSummaryAsync(civilId));
    }
}
