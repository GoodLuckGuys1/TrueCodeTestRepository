using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;
using TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;
using TrueCodeTest.FinanceService.Application.Queries;
using TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

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
            return Unauthorized(new { message = "Невалидный токен " });
        }

        var query = new GetUserCurrenciesQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Currencies);
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> AddFavoriteCurrency([FromBody] AddFavoriteCurrencyRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Невалидный токен" });
        }

        var command = new AddFavoriteCurrencyCommand
        {
            UserId = userId,
            CurrencyName = request.CurrencyName
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Валюта добавлена в избранное" });
    }

    [HttpDelete("favorites")]
    public async Task<IActionResult> RemoveFavoriteCurrency([FromBody] RemoveFavoriteCurrencyRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Невалидный токен" });
        }

        var command = new RemoveFavoriteCurrencyCommand
        {
            UserId = userId,
            CurrencyName = request.CurrencyName
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Валюта удалена из избранного" });
    }
}

public record AddFavoriteCurrencyRequest(string CurrencyName);
public record RemoveFavoriteCurrencyRequest(string CurrencyName);
