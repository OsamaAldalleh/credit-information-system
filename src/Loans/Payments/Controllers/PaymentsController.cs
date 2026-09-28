using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using Loans.Payments.Payloads;
using Loans.Payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loans.Payments.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentsService paymentsService) : ControllerBase
{
    [HttpPost("{loanId}")]
    public async Task<ActionResult<ApiResponse<object>>> UploadPayments(
    Guid loanId,
    [FromBody, Required, MinLength(1), MaxLength(1000)]
    IReadOnlyList<UploadLoanPaymentPayload> payments)
    {
        await paymentsService.UploadPaymentsAsync(loanId, payments);
        return ApiResponseBuilder.Ok();
    }
    
}