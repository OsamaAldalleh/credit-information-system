using Auth.Payloads;
using Auth.Services;
using Common.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserPayload>>> CreateUser(CreateUserPayload payload)
    {
        return ApiResponseBuilder.Ok(await usersService.CreateUserAsync(payload));
    }

    [HttpPut("{userId:guid}/roles")]
    [EndpointDescription("Replaces all of the user's roles with the given ones. Bureau roles are only for bureau users, bank roles only for bank users.")]
    public async Task<ActionResult<ApiResponse<UserPayload>>> UpdateUserRoles(Guid userId, UpdateUserRolesPayload payload)
    {
        return ApiResponseBuilder.Ok(await usersService.UpdateUserRolesAsync(userId, payload));
    }
}
