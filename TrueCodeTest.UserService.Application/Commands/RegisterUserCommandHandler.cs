using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands;

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
            // Check if user already exists
            var existingUser = await _userRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingUser != null)
            {
                return new RegisterUserResult
                {
                    Success = false,
                    ErrorMessage = "User with this name already exists"
                };
            }

            // Hash password
            var hashedPassword = HashPassword(request.Password);

            // Create user
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
            _logger.LogError(ex, "Error registering user");
            return new RegisterUserResult
            {
                Success = false,
                ErrorMessage = "An error occurred while registering the user"
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
