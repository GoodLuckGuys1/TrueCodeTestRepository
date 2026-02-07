using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingUser = await _userRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingUser != null)
            {
                return new RegisterUserResult
                {
                    Success = false,
                    ErrorMessage = "Пользователь с таким именем уже существует"
                };
            }

            var hashedPassword = HashPassword(request.Password);

            var user = new Shared.Domain.Entities.User
            {
                Name = request.Name,
                Password = hashedPassword
            };

            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

            return new RegisterUserResult
            {
                Success = true,
                UserId = createdUser.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка регистрации");
            return new RegisterUserResult
            {
                Success = false,
                ErrorMessage = "Возникла ошибка регистрации пользователя"
            };
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
