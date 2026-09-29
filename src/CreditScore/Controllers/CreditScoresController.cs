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
    public async Task<ActionResult<ApiResponse<CreditScorePayload>>> GetCreditScore(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await creditScoresService.GetCreditScoreAsync(civilId));
    }

    [HttpGet("{civilId}/history")]
    public async Task<ActionResult<ApiResponse<PageResult<CreditScoreHistoryPayload>>>> GetCreditScoreHistory(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await creditScoresService.GetCreditScoreHistoryAsync(civilId, page, pageSize));
    }
}
