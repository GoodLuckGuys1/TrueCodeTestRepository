using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByNameAsync(request.Name, cancellationToken);
            if (user == null)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            var hashedPassword = HashPassword(request.Password);
            if (user.Password != hashedPassword)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            var token = GenerateJwtToken(user);

            return new LoginResult
            {
                Success = true,
                Token = token,
                UserId = user.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private string GenerateJwtToken(Shared.Domain.Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "YourSecretKeyForJWTTokenGenerationThatIsAtLeast32Characters"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TrueCodeTest",
            audience: _configuration["Jwt:Audience"] ?? "TrueCodeTest",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
