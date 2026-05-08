using Taskeen.Domain.Entities;

namespace Taskeen.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> AuthenticateAsync(string identity, string password);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task RegisterRoleAsync(User user);
    Task SoftDeleteWithHistoryAsync(int id);
}
