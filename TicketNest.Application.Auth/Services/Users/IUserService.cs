using TicketNest.Domain.Auth.Models;
using TicketNest.Domain.Auth.Models.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Auth.Services.Users;

public interface IUserService
{
    Task<Result<User, Error>> Register(string login, string password, UserRole? role, CancellationToken ct = default);

    Task<Result<string, Error>> Login(string login, string password, CancellationToken ct = default);
}
