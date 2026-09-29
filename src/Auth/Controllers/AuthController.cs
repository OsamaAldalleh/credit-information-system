using Auth.Payloads;
using Auth.Services;
using Common.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [EndpointDescription("Returns a JWT valid for 60 minutes; send it as \"Authorization: Bearer <token>\". Limited to 10 attempts per minute per IP.")]
    public async Task<ActionResult<ApiResponse<TokenPayload>>> Login(LoginPayload payload)
    {
        return ApiResponseBuilder.Ok(await authService.LoginAsync(payload));
    }
}
