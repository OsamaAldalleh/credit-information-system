using System.ComponentModel.DataAnnotations;
using Common.Api.Responses;
using Customers.Models;
using Customers.Payloads;
using Customers.Services;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(CustomersService customersService) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerPayload>>> CreateCustomer(CustomerPayload payload)
    {
        var customer = await customersService.CreateCustomerAsync(payload);
        var location = Url.RouteUrl(
            nameof(GetCustomer),
            new { civilId = customer.CivilId }
        )!;
        return ApiResponseBuilder.Created(location, customer);
    }

    [HttpGet("{civilId}", Name = nameof(GetCustomer))]
    public async Task<ActionResult<ApiResponse<CustomerPayload>>> GetCustomer(string civilId)
    {
        return ApiResponseBuilder.Ok(await customersService.GetCustomerAsync(civilId));
    }

    [HttpPatch("{civilId}/loan-eligibility")]
    public async Task<ActionResult<ApiResponse<CustomerPayload>>> UpdateCustomerLoanEligibility(string civilId, UpdateEligibilityPayload payload)
    {
        return ApiResponseBuilder.Ok(await customersService.UpdateCustomerLoanEligibilityAsync(civilId, payload));
    }

    [HttpGet("{civilId}/loan-eligibility/history")]
    public async Task<ActionResult<ApiResponse<CustomerLoanEligibilityHistoryResponse>>> GetLoanEligibilityHistory(
        string civilId,
        [FromQuery, Range(0, int.MaxValue)] int page = 0,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        return ApiResponseBuilder.Ok(await customersService.GetLoanEligibilityHistoryAsync(civilId, page, pageSize));
    }

}
