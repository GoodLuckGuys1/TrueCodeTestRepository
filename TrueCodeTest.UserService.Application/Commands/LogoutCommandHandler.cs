using MediatR;
using Microsoft.Extensions.Logging;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        ILogger<LogoutCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // In a stateless JWT system, logout is typically handled client-side
            // by removing the token. However, we can verify the user exists.
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
            {
                return new LogoutResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // In a real system, you might want to maintain a token blacklist
            // For now, we'll just return success
            return new LogoutResult
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return new LogoutResult
            {
                Success = false,
                Message = "An error occurred during logout"
            };
        }
    }
}
