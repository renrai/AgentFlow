using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AgentFlowDbContext dbContext) : IUserRepository
{
    public void Add(User user) => dbContext.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        return dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        return dbContext.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    private static string NormalizeEmail(string email)
        => string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();
}
