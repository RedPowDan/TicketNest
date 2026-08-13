using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Users;

public interface IUserFactory
{
    Task<Result<User, Error>> Create(string login, string password, UserRole? role, CancellationToken ct = default);
}