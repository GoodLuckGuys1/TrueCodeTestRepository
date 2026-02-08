using MediatR;
using Microsoft.Extensions.Logging;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands.Logout;

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
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
            {
                return new LogoutResult
                {
                    Success = false,
                    Message = "Пользователь не найден"
                };
            }

            return new LogoutResult
            {
                Success = true,
                Message = "Успешное разлогинивание"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка");
            return new LogoutResult
            {
                Success = false,
                Message = "Ошибка выхода пользователя"
            };
        }
    }
}
