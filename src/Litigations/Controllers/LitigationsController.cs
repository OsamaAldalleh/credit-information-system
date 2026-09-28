using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using Litigations.Models;
using Litigations.Payloads;
using Litigations.Services;
using Microsoft.AspNetCore.Mvc;

namespace Litigations.Controllers;

[ApiController]
[Route("api/litigations")]
public class LitigationsController(LitigationsService litigationsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LitigationPayload>>> CreateLitigation(CreateLitigationPayload payload)
    {
        var litigation = await litigationsService.CreateLitigationAsync(payload);
        var location = Url.RouteUrl(
            nameof(GetLitigation),
            new { litigationId = litigation.Id }
        )!;
        return ApiResponseBuilder.Created(location, litigation);
    }

    [HttpPatch("cases/{litigationId}/status")]
    public async Task<ActionResult<ApiResponse<LitigationPayload>>> UpdateLitigationStatus(
        Guid litigationId,
        UpdateLitigationStatusPayload payload)
    {
        return ApiResponseBuilder.Ok(await litigationsService.UpdateLitigationStatusAsync(litigationId, payload));
    }

    [HttpGet("cases/{litigationId}", Name = nameof(GetLitigation))]
    public async Task<ActionResult<ApiResponse<LitigationPayload>>> GetLitigation(Guid litigationId)
    {
        return ApiResponseBuilder.Ok(await litigationsService.GetLitigationAsync(litigationId));
    }

    [HttpGet("loan/{loanId}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LitigationPayload>>>> GetLoanLitigations(Guid loanId)
    {
        return ApiResponseBuilder.Ok(await litigationsService.GetLoanLitigationsAsync(loanId));
    }

    [HttpGet("{civilId}")]
    public async Task<ActionResult<ApiResponse<PageResult<LitigationPayload>>>> GetCustomerLitigations(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId,
        [FromQuery, EnumDataType(typeof(LitigationStatus))] LitigationStatus? status = null,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await litigationsService.GetCustomerLitigationsAsync(civilId, status, page, pageSize));
    }

    // Internal: used by the Loans service, never mapped in the gateway.
    [HttpGet("{civilId}/legal-loans/ids")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Guid>>>> GetLegalLoanIds(
        [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")] string civilId)
    {
        return ApiResponseBuilder.Ok(await litigationsService.GetLegalLoanIdsAsync(civilId));
    }
}
