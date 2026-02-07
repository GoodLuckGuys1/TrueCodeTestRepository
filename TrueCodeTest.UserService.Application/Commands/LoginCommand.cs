using MediatR;

namespace TrueCodeTest.UserService.Application.Commands;

public class LoginCommand : IRequest<LoginResult>
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public int? UserId { get; set; }
    public string? ErrorMessage { get; set; }
}
