using MediatR;
using TrueCodeTest.Shared.Domain.Common;

namespace TrueCodeTest.UserService.Application.Commands.Logout;

public class LogoutCommand : IRequest<LogoutResult>
{
    public int UserId { get; set; }
}

public class LogoutResult : ResultBase
{
    public string? Message { get; set; }
}
