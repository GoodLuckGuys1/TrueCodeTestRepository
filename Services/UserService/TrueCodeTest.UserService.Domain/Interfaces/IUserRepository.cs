using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.UserService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
