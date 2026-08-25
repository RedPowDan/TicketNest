using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.Domain.Auth.Repositories;

public interface IUserRepository
{
    Task Save(User user, CancellationToken ct = default);

    ValueTask<User?> Get(Guid id, CancellationToken ct = default);

    ValueTask<User?> GetByLogin(string login, CancellationToken ct = default);
}