using TicketNest.Domain.Auth.Models;
using TicketNest.Domain.Auth.Models.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Auth.Services.Users;

public interface IUserFactory
{
    Task<Result<User, Error>> Create(string login, string password, UserRole? role, CancellationToken ct = default);
}