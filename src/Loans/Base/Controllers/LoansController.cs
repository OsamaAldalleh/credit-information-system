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
    public async Task<ActionResult<ApiResponse<PageResult<LoanPayload>>>> GetCustomerLoans(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LoanStatus))] LoanStatus? loanStatus = null,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCustomerLoansAsync(civilId, loanStatus, page, pageSize));
    }

    [HttpGet("{civilId}/total")]
    public async Task<ActionResult<ApiResponse<TotalCustomerLoansAmountPayload>>> GetCustomerTotalLoansAmount(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LoanStatus))] LoanStatus? loanStatus = LoanStatus.Open)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCustomerTotalLoansAmountAsync(civilId, loanStatus));
    }

    [HttpGet("{civilId}/delinquent")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DelinquentLoanPayload>>>> GetDelinquentLoans(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetDelinquentLoansAsync(civilId));
    }

    [HttpGet("{civilId}/next-payment")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NextPaymentPayload>>>> GetNextPayments(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetNextPaymentsAsync(civilId));
    }

    // Internal: used by the CreditScore service, never mapped in the gateway.
    [HttpGet("{civilId}/credit-summary")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LoanCreditSummaryPayload>>>> GetCreditSummary(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await loansService.GetCreditSummaryAsync(civilId));
    }
}
