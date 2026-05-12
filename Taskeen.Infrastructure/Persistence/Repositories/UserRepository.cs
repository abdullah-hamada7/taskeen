using Microsoft.EntityFrameworkCore;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Repositories;
using Taskeen.Application.Interfaces;

namespace Taskeen.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskeenDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UserRepository(TaskeenDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<User?> AuthenticateAsync(string identity, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.IdentityNumber == identity && !u.IsDeleted);
        
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task RegisterRoleAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteWithHistoryAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
