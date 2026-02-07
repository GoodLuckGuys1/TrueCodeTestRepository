using MediatR;
using TrueCodeTest.Shared.Domain.Common;

namespace TrueCodeTest.UserService.Application.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterUserResult : ResultBase
{
    public int UserId { get; set; }
}
