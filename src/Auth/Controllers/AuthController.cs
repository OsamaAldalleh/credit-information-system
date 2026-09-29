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
    public async Task<ActionResult<ApiResponse<TokenPayload>>> Login(LoginPayload payload)
    {
        return ApiResponseBuilder.Ok(await authService.LoginAsync(payload));
    }
}
