using Auth.Payloads;
using Auth.Services;
using Common.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/institutions")]
public class InstitutionsController(InstitutionsService institutionsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InstitutionPayload>>> CreateInstitution(CreateInstitutionPayload payload)
    {
        return ApiResponseBuilder.Ok(await institutionsService.CreateInstitutionAsync(payload));
    }
}
