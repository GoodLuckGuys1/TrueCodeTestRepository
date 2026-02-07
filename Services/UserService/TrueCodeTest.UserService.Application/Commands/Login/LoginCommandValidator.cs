using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.UserService.Application.Commands.Login;

public class LoginCommandValidator : BaseValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        ValidateUserName(x => x.Name);
        ValidatePassword(x => x.Password);
    }
}
