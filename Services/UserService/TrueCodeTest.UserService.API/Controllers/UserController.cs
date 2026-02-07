using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Application.Commands.Login;
using TrueCodeTest.UserService.Application.Commands.Logout;
using TrueCodeTest.UserService.Application.Commands.RegisterUser;

namespace TrueCodeTest.UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { userId = result.UserId, message = "Пользователь зарегистрирован" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new { token = result.Token, userId = result.UserId });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Невалидный токен" });
        }

        var command = new LogoutCommand { UserId = userId };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
