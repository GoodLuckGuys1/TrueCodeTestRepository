using MediatR;
using TrueCodeTest.Shared.Domain.Common;

namespace TrueCodeTest.UserService.Application.Commands.Login;

public class LoginCommand : IRequest<LoginResult>
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult : ResultBase
{
    public string? Token { get; set; }
    public int? UserId { get; set; }
}
