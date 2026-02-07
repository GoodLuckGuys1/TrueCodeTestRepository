using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrueCodeTest.FinanceService.Application.Queries;

namespace TrueCodeTest.FinanceService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("currencies")]
    public async Task<IActionResult> GetUserCurrencies()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var query = new GetUserCurrenciesQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Currencies);
    }
}
