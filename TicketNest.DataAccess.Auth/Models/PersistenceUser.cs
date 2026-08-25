using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.DataAccess.Auth.Models;

internal sealed class PersistenceUser
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }
}