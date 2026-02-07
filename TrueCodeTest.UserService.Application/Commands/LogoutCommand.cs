using MediatR;

namespace TrueCodeTest.UserService.Application.Commands;

public class LogoutCommand : IRequest<LogoutResult>
{
    public int UserId { get; set; }
}

public class LogoutResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
