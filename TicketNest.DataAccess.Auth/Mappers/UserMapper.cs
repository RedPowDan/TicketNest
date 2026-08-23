using TicketNest.DataAccess.Auth.Models;
using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.DataAccess.Auth.Mappers;

internal static class UserMapper
{
    public static User ToDomain(PersistenceUser source)
    {
        Ensure.NotNull(source, nameof(source));

        return User.LoadFromStorage(
            id: source.Id,
            login: source.Login,
            passwordHash: source.PasswordHash,
            role: source.Role);
    }

    public static void Map(User source, PersistenceUser target)
    {
        Ensure.NotNull(source, nameof(source));
        Ensure.NotNull(target, nameof(target));

        target.Id = source.Id;
        target.Login = source.Login;
        target.PasswordHash = source.PasswordHash;
        target.Role = source.Role;
    }

    public static PersistenceUser ToPersistence(User source)
    {
        Ensure.NotNull(source, nameof(source));

        var persistenceUser = new PersistenceUser();

        Map(source, persistenceUser);

        return persistenceUser;
    }
}