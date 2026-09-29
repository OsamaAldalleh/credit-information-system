using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using CreditScore.Payloads;
using CreditScore.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreditScore.Controllers;

[ApiController]
[Route("api/credit-scores")]
public class CreditScoresController(CreditScoresService creditScoresService) : ControllerBase
{
    [HttpGet("{civilId}")]
    [EndpointDescription("F: more than 3 overdue installments, a guilty verdict in the last year (3 years for loans of 10,000 or more), " +
        "or overdue installments with a loan in litigation. C: 1 to 3 overdue installments. A: has open loans and none of the above. " +
        "B: everything else, including customers without loans. Recalculated on every loan, payment or litigation change, and nightly at 01:00 UTC.")]
    public async Task<ActionResult<ApiResponse<CreditScorePayload>>> GetCreditScore(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await creditScoresService.GetCreditScoreAsync(civilId));
    }

    [HttpGet("{civilId}/history")]
    [EndpointDescription("One entry per grade change, with what triggered it.")]
    public async Task<ActionResult<ApiResponse<PageResult<CreditScoreHistoryPayload>>>> GetCreditScoreHistory(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await creditScoresService.GetCreditScoreHistoryAsync(civilId, page, pageSize));
    }
}
