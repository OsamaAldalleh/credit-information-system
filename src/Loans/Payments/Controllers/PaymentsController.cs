using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using Loans.Base.Models;
using Loans.Payments.Payloads;
using Loans.Payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Payments.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentsService paymentsService) : ControllerBase
{
    [HttpPost("{loanId}")]
    [EndpointDescription("Adds 1 to 1000 payments to the loan. Payments settle the oldest unpaid installments first. payment_reference must be unique within the bank.")]
    public async Task<ActionResult<ApiResponse<object>>> UploadPayments(
    Guid loanId,
    [FromBody, Required, MinLength(1), MaxLength(1000)]
    IReadOnlyList<UploadLoanPaymentPayload> payments)
    {
        await paymentsService.UploadPaymentsAsync(loanId, payments);
        return ApiResponseBuilder.Ok();
    }

    [HttpGet("loan/{loanId}")]
    public async Task<ActionResult<ApiResponse<PageResult<LoanPaymentPayload>>>> GetLoanPayments(
        Guid loanId,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, RegularExpression("^(asc|desc)$", ErrorMessage = "sort must be asc or desc")] string sort = "desc")
    {
        return ApiResponseBuilder.Ok(await paymentsService.GetLoanPaymentsAsync(loanId, page, pageSize, sort));
    }

    [HttpGet("reference/{paymentReference}")]
    [EndpointDescription("Bank users only find their own bank's payments. References are unique per bank, so for bureau users the latest match is returned.")]
    public async Task<ActionResult<ApiResponse<LoanPaymentPayload>>> GetPaymentByReference(
        [Required, StringLength(64)] string paymentReference)
    {
        return ApiResponseBuilder.Ok(await paymentsService.GetPaymentByReferenceAsync(paymentReference));
    }

    [HttpGet("{paymentId}")]
    public async Task<ActionResult<ApiResponse<LoanPaymentPayload>>> GetPayment(Guid paymentId)
    {
        return ApiResponseBuilder.Ok(await paymentsService.GetPaymentAsync(paymentId));
    }

    [HttpGet("customer/{civilId}")]
    public async Task<ActionResult<ApiResponse<PageResult<LoanPaymentPayload>>>> GetCustomerPayments(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LoanStatus))] LoanStatus? loanStatus = null,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, RegularExpression("^(asc|desc)$", ErrorMessage = "sort must be asc or desc")] string sort = "desc")
    {
        return ApiResponseBuilder.Ok(await paymentsService.GetCustomerPaymentsAsync(civilId, loanStatus, page, pageSize, sort));
    }

    [HttpGet("customer/{civilId}/latest")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LoanPaymentPayload>>>> GetLatestCustomerPayments(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, Range(1, 100)] int limit = 5)
    {
        return ApiResponseBuilder.Ok(await paymentsService.GetLatestCustomerPaymentsAsync(civilId, limit));
    }
}
