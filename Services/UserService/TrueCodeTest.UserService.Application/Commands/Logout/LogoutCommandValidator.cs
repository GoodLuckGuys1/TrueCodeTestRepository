using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.UserService.Application.Commands.Logout;

public class LogoutCommandValidator : BaseValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        ValidateUserId(x => x.UserId);
    }
}
