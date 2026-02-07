using MediatR;

namespace TrueCodeTest.UserService.Application.Commands;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterUserResult
{
    public int UserId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
