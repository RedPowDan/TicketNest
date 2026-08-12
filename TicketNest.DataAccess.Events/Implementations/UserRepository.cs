using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.Domain.Models.Users;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Events.Implementations;

internal sealed class UserRepository(EventsDbContext dbContext) : IUserRepository
{
    public async Task Save(User user, CancellationToken ct = default)
    {
        Ensure.NotNull(user, nameof(user));

        var persistenceUser = await dbContext
            .Users
            .FindAsync([user.Id], cancellationToken: ct);
        if (persistenceUser != null)
        {
            UserMapper.Map(user, persistenceUser);
        }
        else
        {
            persistenceUser = UserMapper.ToPersistence(user);
            dbContext.Users.Add(persistenceUser);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async ValueTask<User?> Get(Guid id, CancellationToken ct = default)
    {
        var persistenceUser = await dbContext
            .Users
            .FindAsync([id], cancellationToken: ct);

        return persistenceUser == null
            ? null
            : UserMapper.ToDomain(persistenceUser);
    }

    public async ValueTask<User?> GetByLogin(string login, CancellationToken ct = default)
    {
        var persistenceUser = await dbContext
            .Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Login == login, cancellationToken: ct);

        return persistenceUser == null
            ? null
            : UserMapper.ToDomain(persistenceUser);
    }
}