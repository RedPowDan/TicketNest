using TicketNest.Domain.Models.Users;

namespace TicketNest.DataAccess.Events.Models;

internal sealed class PersistenceUser
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }
}